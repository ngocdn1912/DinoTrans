using DinoTrans.Shared.Contracts;
using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.DTOs.SearchDTO;
using DinoTrans.Shared.DTOs.TenderSteps;
using DinoTrans.Shared.DTOs.UserResponse;
using DinoTrans.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DinoTrans.Shared.DTOs.ServiceResponses;

namespace DinoTrans.Shared.Services.Interfaces
{
    public interface IUserService
    {
        Task<string> GetUserRole(int userId); 
        Task<string> GetCurrentUserRole(ApplicationUser user);
        Task<GeneralResponse> CreateAccount(UserDTO userDTO);
        Task<LoginResponse> LoginAccount(LoginDTO loginDTO);
        Task<ResponseModel<UserInfoResponseDTO>> GetAllUserInfo(GetAllUserInfoDTO userInfo);
        Task<GeneralResponse> ChangeUserPassword(ChangePasswordDTO changePasswordDTO);
        Task<GeneralResponse> UpdateIsAdminConfirm();
        Task<ResponseModel<CompanyRoleEnum>> GetCompanyRole(int CompanyId);
        ResponseModel<ApplicationUser> GetUserById(int UserId);
        Task<ResponseModel<ApplicationUser>> GetUserByIdAsync(int UserId);
        Task<GeneralResponse> CreateAccountForUserOfCompany(CreateAccountForUserOfCompany dto, ApplicationUser _currentCompanyShipperAdmin);
        Task<ResponseModel<List<GetEmployeeOfACompany>>> GetAllEmployeesOfACompany(SearchModel dto, ApplicationUser _currentCompanyShipperAdmin);
        Task<GeneralResponse> UpdateAccountForUserOfCompany(UpdateAccountForUserOfCompany dto);
        Task<GeneralResponse> DeleteUserAccount(int UserId);
    }
}
