using DinoTrans.Shared.Data;
using DinoTrans.Shared.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.Repositories.Implements
{
    public class UserRoleRepository : Repository<IdentityUserRole<int>>, IUserRoleRepository
    {
        public UserRoleRepository(DinoTransDbContext context) : base(context)
        {
        }
    }
}
