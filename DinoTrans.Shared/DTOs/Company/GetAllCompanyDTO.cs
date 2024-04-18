using DinoTrans.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.DTOs.Company
{
    public class GetAllCompanyDTO : Entities.Company
    {
        public List<ApplicationUser> UsersInCompany { get; set; }
    }
}
