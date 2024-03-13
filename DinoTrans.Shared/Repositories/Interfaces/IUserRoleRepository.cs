using DinoTrans.Shared.Data;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.Repositories.Interfaces
{
    public interface IUserRoleRepository : IRepository<IdentityUserRole<int>>
    {
    }
}
