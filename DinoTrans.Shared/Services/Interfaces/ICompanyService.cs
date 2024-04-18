using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.DTOs.Company;
using DinoTrans.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DinoTrans.Shared.DTOs.ServiceResponses;

namespace DinoTrans.Shared.Services.Interfaces
{
    public interface ICompanyService
    {
        public Task<ResponseModel<Company>> GetCompanyByCurrentUserId(ApplicationUser user);
        public Task<GeneralResponse> UpdateCompanyInforByAdminOfCompany(UpdateCompanyDTO dto);
        public Task<ResponseModel<Company>> GetCompanyByCompanyId(int CompanyId);
        public Task<ResponseModel<List<GetAllCompanyDTO>>> GetAllCompaniesByAdmin();

    }
}
