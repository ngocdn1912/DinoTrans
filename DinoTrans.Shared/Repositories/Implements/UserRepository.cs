using DinoTrans.Shared.Data;
using DinoTrans.Shared.Entities;
using DinoTrans.Shared.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.Repositories.Implements
{
    public class UserRepository : Repository<ApplicationUser>, IUserRepository
    {
        public UserRepository(DinoTransDbContext context) : base(context)
        {
        }
    }
}
