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

        public CompanyService(ICompanyRepository companyRepository)
        {
            _companyRepository = companyRepository;
        }

        public async Task<ResponseModel<Company>> GetCompanyById(ApplicationUser user)
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
                .Where(c => c.Id == user.CompanyId)
                .FirstOrDefaultAsync();

            if (company == null)
            {
                return new GeneralResponse(false, $"Không tìm thấy công ty với Id = {user.CompanyId}");
            }

            company.PhoneNumber = dto.PhoneNumber;
            company.Address = dto.Address;
            company.CompanyName = dto.CompanyName;
            company.Email = dto.Email;
            _companyRepository.Update(company);
            _companyRepository.SaveChange();
            return new GeneralResponse(true, "Cập nhật dữ liệu công ty thành công");
        }
    }
}
