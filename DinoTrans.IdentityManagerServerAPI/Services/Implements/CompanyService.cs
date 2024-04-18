using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.DTOs.Company;
using DinoTrans.Shared.Entities;
using DinoTrans.Shared.Repositories.Interfaces;
using DinoTrans.Shared.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;
using static DinoTrans.Shared.DTOs.ServiceResponses;

namespace DinoTrans.IdentityManagerServerAPI.Services.Implements
{
    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IUserRepository _userRepository;

        public CompanyService(ICompanyRepository companyRepository,
            IUserRepository userRepository)
        {
            _companyRepository = companyRepository;
            _userRepository = userRepository;
        }

        public async Task<ResponseModel<List<GetAllCompanyDTO>>> GetAllCompaniesByAdmin()
        {
            var company = (from c in _companyRepository.AsNoTracking()
                          join u in _userRepository.AsNoTracking() on c.Id equals u.CompanyId
                          select new
                          {
                              CompanyId = c.Id,
                              CompanyName = c.CompanyName,
                              Email = c.Email,
                              PhoneNumber = c.PhoneNumber,
                              Role = c.Role,
                              Address = c.Address,
                              ShipperFeePercentage = c.ShipperFeePercentage,
                              CarrierFeePercentage = c.CarrierFeePercentage,
                              IsAdminConfirm = c.IsAdminConfirm,
                              UserId = u.Id,
                              FirstName = u.FirstName,
                              LastName = u.LastName,
                              UserPhoneNumber = u.PhoneNumber,
                              UserAddress = u.Address,
                              UserEmail = u.Email,
                          })
                          .GroupBy(c => new {c.CompanyId, c.CompanyName, c.Email, c.PhoneNumber, c.Role, c.Address, c.ShipperFeePercentage
                          , c.CarrierFeePercentage, c.IsAdminConfirm})
                          .Select(c => new GetAllCompanyDTO
                          {
                              Id = c.Key.CompanyId,
                              CompanyName = c.Key.CompanyName,
                              Email = c.Key.Email,
                              PhoneNumber = c.Key.PhoneNumber,
                              Role = c.Key.Role,
                              Address = c.Key.Address,
                              ShipperFeePercentage = c.Key.ShipperFeePercentage,
                              CarrierFeePercentage = c.Key.CarrierFeePercentage,
                              IsAdminConfirm = c.Key.IsAdminConfirm,
                              UsersInCompany = c.Select(u => new ApplicationUser
                              { 
                                  FirstName = u.FirstName,
                                  LastName = u.LastName,
                                  PhoneNumber = u.UserPhoneNumber,
                                  Address = u.UserAddress,
                                  Email = u.UserEmail,
                              }).ToList(),
                          })
                          .ToList();                          

            return new ResponseModel<List<GetAllCompanyDTO>>
            {
                Data = company,
                Success = true
            };
        }

        public async Task<ResponseModel<Company>> GetCompanyByCompanyId(int CompanyId)
        {
            var company = await _companyRepository
                .AsNoTracking()
                .Where(c => c.Id == CompanyId)
                .FirstOrDefaultAsync();

            if (company == null)
            {
                return new ResponseModel<Company>
                {
                    Success = false,
                    Message = $"Không tìm thấy công ty với Id = {CompanyId}"
                };
            }
            return new ResponseModel<Company>
            {
                Success = true,
                Data = company
            };
        }

        public async Task<ResponseModel<Company>> GetCompanyByCurrentUserId(ApplicationUser user)
        {
            var company = await _companyRepository
                .AsNoTracking()
                .Where(c => c.Id == user.CompanyId)
                .FirstOrDefaultAsync();

            if(company == null) 
            {
                return new ResponseModel<Company>
                {
                    Success = false,
                    Message = $"Không tìm thấy công ty với Id = {user.CompanyId}"
                };
            }
            return new ResponseModel<Company>
            {
                Success = true,
                Data = company
            };
        }

        public async Task<ServiceResponses.GeneralResponse> UpdateCompanyInforByAdminOfCompany(UpdateCompanyDTO dto, ApplicationUser user)
        {
            var company = await _companyRepository
            .AsNoTracking()
                .Where(c => c.Id == dto.CompanyId)
                .FirstOrDefaultAsync();

            var currentUserCompany = await _companyRepository
                .AsNoTracking()
                .Where(c => c.Id == user.CompanyId)
                .FirstOrDefaultAsync();

            if (company == null)
            {
                return new GeneralResponse(false, $"Không tìm thấy công ty với Id = {dto.CompanyId}");
            }

            company.PhoneNumber = dto.PhoneNumber;
            company.Address = dto.Address;
            company.CompanyName = dto.CompanyName;
            company.Email = dto.Email;

            if(currentUserCompany.Role == Shared.Contracts.CompanyRoleEnum.Admin)
            {
                company.ShipperFeePercentage = dto.ShipperFeePercentage != null ? dto.ShipperFeePercentage.Value : 0 ;
                company.CarrierFeePercentage = dto.CarrierFeePercentage != null ? dto.CarrierFeePercentage.Value : 0;
                company.IsAdminConfirm = dto.IsActive != null ? dto.IsActive : false;
            }    
            _companyRepository.Update(company);
            _companyRepository.SaveChange();
            return new GeneralResponse(true, "Cập nhật dữ liệu công ty thành công");
        }
    }
}
