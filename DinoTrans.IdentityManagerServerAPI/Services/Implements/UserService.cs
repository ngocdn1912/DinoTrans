using DinoTrans.Shared.Contracts;
using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.Entities;
using DinoTrans.Shared.Repositories.Implements;
using DinoTrans.Shared.Repositories.Interfaces;
using DinoTrans.Shared.Services.Interfaces;
using DinoTrans.Shared.DTOs.UserResponse;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static DinoTrans.Shared.DTOs.ServiceResponses;
using Microsoft.EntityFrameworkCore;

namespace DinoTrans.IdentityManagerServerAPI.Services.Implements
{
    public class UserService : IUserService
    {
        // Inject các thành phần cần thiết từ DI container
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICompanyRepository _companyRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRoleRepository _userRoleRepository;

        public UserService(IConfiguration configuration,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IUnitOfWork unitOfWork,
            ICompanyRepository companyRepository,
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IUserRoleRepository userRoleRepository)
        {
            _userManager = userManager;
            _configuration = configuration;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
            _companyRepository = companyRepository;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
        }
        public async Task<GeneralResponse> ChangeUserPassword(ChangePasswordDTO changePasswordDTO)
        {
            var getUser = await _userManager.FindByIdAsync(changePasswordDTO.UserId.ToString());
            if (getUser == null)
            {
                return new GeneralResponse(false, "Can't find user to change password");
            }
            var result = await _userManager.ChangePasswordAsync(getUser, changePasswordDTO.CurrentPassword, changePasswordDTO.NewPassword);
            if (!result.Succeeded)
            {
                var errors = "";
                foreach (var itemError in result.Errors)
                {
                    errors += itemError.Description.ToString() + "\n";
                }
                return new GeneralResponse(false, errors);
            }
            return new GeneralResponse(true, "Change password successfully");
        }
        // Phương thức tạo tài khoản người dùng do admin của công ty
        public async Task<GeneralResponse> CreateAccount(UserDTO userDTO)
        {
            if (userDTO is null) return new GeneralResponse(false, "Model is empty");
            try
            {
                _unitOfWork.BeginTransaction();
                // Kiểm tra xem công ty đã tồn tại trong cơ sở dữ liệu chưa
                var existingCompany = _companyRepository
                    .AsNoTracking()
                    .FirstOrDefault(c => c.CompanyName == userDTO.CompanyName && c.Email == userDTO.CompanyEmail);

                if (existingCompany != null)
                {
                    _unitOfWork.Rollback();
                    return new GeneralResponse(false, "Company already registered");
                }
                // Thêm công ty mới
                var newCompany = new Company
                {
                    CompanyName = userDTO.CompanyName,
                    Email = userDTO.CompanyEmail,
                    PhoneNumber = userDTO.CompanyPhoneNumber,
                    Role = userDTO.CompanyRole,
                    Address = userDTO.CompanyAddress,
                    IsAdminConfirm = false
                };
                _companyRepository.Add(newCompany);
                _companyRepository.SaveChange();


                // Thêm người dùng mới
                var newUser = new ApplicationUser()
                {
                    FirstName = userDTO.FirstName,
                    LastName = userDTO.LastName,
                    Email = userDTO.Email,
                    PasswordHash = userDTO.Password,
                    UserName = userDTO.Email,
                    PhoneNumber = userDTO.PhoneNumber,
                    Address = userDTO.Address,
                    CompanyId = newCompany.Id
                };

                // Kiểm tra xem người dùng đã đăng ký trước đó chưa
                var user = await _userManager.FindByEmailAsync(newUser.Email);
                if (user is not null) return new GeneralResponse(false, "Tài khoản đã được đăng ký");

                var createUser = await _userManager.CreateAsync(newUser!, userDTO.Password);
                if (!createUser.Succeeded)
                {
                    _unitOfWork.Rollback();
                    var allErrors = "";
                    foreach (var item in createUser.Errors)
                    {
                        allErrors += item.Description.ToString() + " ";
                    }
                    return new GeneralResponse(false, allErrors);
                }

                // Thêm vai trò cho người dùng

                if (userDTO.Role == "Quản trị viên công ty") userDTO.Role = Role.CompanyAdministrator;
                else if (userDTO.Role == "Admin của DinoTrans")
                    userDTO.Role = Role.DinoTransAdmin;
                var findRole = await _roleManager.FindByNameAsync(userDTO.Role);
                if (findRole is null)
                {
                    _unitOfWork.Rollback();
                    return new GeneralResponse(false, "Role name doesn't exist");
                }
                /*if (findRole.Name != Role.CompanyAdministrator)
                {
                    return new GeneralResponse(false, "Forbidden");
                }*/
                await _userManager.AddToRoleAsync(newUser, userDTO.Role);

                _unitOfWork.SaveChanges();
                _unitOfWork.Commit();
                return new GeneralResponse(true, "Đăng ký thành công");
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                return new GeneralResponse(false, ex.Message);
            }
        }

        // Phương thức đăng nhập
        public async Task<LoginResponse> LoginAccount(LoginDTO loginDTO)
        {
            if (loginDTO == null)
                return new LoginResponse(false, null!, "Email/Mật khẩu không được để trống");
            var getUser = await _userManager.FindByEmailAsync(loginDTO.Email);
            if (getUser is null)
                return new LoginResponse(false, null!, "Không tìm thấy tài khoản");
            bool checkUserPasswords = await _userManager.CheckPasswordAsync(getUser, loginDTO.Password);
            if (!checkUserPasswords)
                return new LoginResponse(false, null!, "Email/Mật khẩu không đúng");

            var company = _companyRepository
                .AsNoTracking()
                .Where(c => c.Id == getUser.CompanyId)
                .FirstOrDefault();
            if ((bool)!company.IsAdminConfirm)
                return new LoginResponse(false, null, "Công ty của bạn chưa được duyệt bởi quản trị viên hệ thống");
            var getUserRole = await _userManager.GetRolesAsync(getUser);

            var userSession = new UserSession(getUser.Id.ToString(), getUser.FirstName + " " + getUser.LastName, getUser.Email, getUserRole.First(), getUser.CompanyId.ToString());
            string token = GenerateToken(userSession);
            return new LoginResponse(true, token!, "Đăng nhập thành công");
        }

        // Phương thức sinh token JWT
        private string GenerateToken(UserSession user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var userClaims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("CompanyId",user.CompanyId)
            };
            var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: userClaims,
            expires: DateTime.Now.AddHours(2),
            signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<ResponseModel<UserInfoResponseDTO>> GetAllUserInfo(GetAllUserInfoDTO userInfo)
        {
            var user = _userRepository
                .AsNoTracking()
                .Where(u => u.Id == userInfo.UserId)
                .FirstOrDefault();
            if (user == null)
            {
                return new ResponseModel<UserInfoResponseDTO>
                {
                    Success = false,
                    ResponseCode = "404",
                };
            }

            var company = _companyRepository
                .AsNoTracking()
                .Where(c => c.Id == userInfo.CompanyId)
                .FirstOrDefault();

            var roleId = _userRoleRepository
                .AsNoTracking()
                .Where(u => u.UserId == userInfo.UserId)
                .Select(u => u.RoleId)
                .FirstOrDefault();
            var roleName = _roleRepository
                .AsNoTracking()
                .Where(r => r.Id == roleId)
                .Select(r => r.Name)
                .FirstOrDefault();
            var response = new UserInfoResponseDTO
            {
                UserInfo = new UserInfo
                {
                    UserId = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address,
                    Role = roleName
                },
                Company = new CompanyInfo
                {
                    CompanyId = company.Id,
                    CompanyName = company.CompanyName,
                    CompanyEmail = company.Email,
                    CompanyPhoneNumber = company.PhoneNumber,
                    CompanyRole = company.Role,
                    CompanyRoleName = company.Role.ToString(),
                    CompanyAddress = company.Address,
                }
            };

            return new ResponseModel<UserInfoResponseDTO>
            {
                Success = true,
                ResponseCode = "200",
                Data = response
            };
        }

        public async Task<GeneralResponse> UpdateIsAdminConfirm()
        {
            try
            {
                var companiesToUpdate = await _companyRepository
                    .AsNoTracking()
                    .Where(c => c.IsAdminConfirm == false)
                    .ToListAsync();
                if (companiesToUpdate.Count == 0)
                {
                    return new GeneralResponse(false, "Tất cả công ty đã được xác nhận rồi");
                }
                // Cập nhật trường IsAdminConfirm thành true cho từng công ty
                foreach (var company in companiesToUpdate)
                {
                    company.IsAdminConfirm = true;
                    _companyRepository.Update(company);
                }
                _companyRepository.SaveChange();
                return new GeneralResponse(true, "Cập nhật thành công");
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu có
                return new GeneralResponse(false, $"Đã xảy ra lỗi: {ex.Message}");
            }
        }
        //khi kiểu dữ liệu trả về có bao gồm async Task<T> bên trong hàm có thể sẽ có 1 thao tác phải xử lý bất đồng bộ
        public async Task<ResponseModel<CompanyRoleEnum>> GetCompanyRole(int CompanyId)
        {
            var getCompanyRole = await _companyRepository
            .AsNoTracking()
                .Where(c => c.Id == CompanyId)
                .Select(c => c.Role)
                .FirstOrDefaultAsync();
            return new ResponseModel<CompanyRoleEnum>
            {
                Data = getCompanyRole,
                Success = true
            };
        }

        public async Task<ResponseModel<ApplicationUser>> GetUserById(int UserId)
        {
            var user = await _userRepository.AsNoTracking().Include(u => u.Company).Where(u => u.Id == UserId).FirstOrDefaultAsync();
            if (user == null)
            {
                return new ResponseModel<ApplicationUser>
                {
                    Success = false,
                    Message = $"Can't find User with Id = {UserId}"
                };
            }
            return new ResponseModel<ApplicationUser>
            {
                Data = user,
                Success = true
            };
        }
    }
}
