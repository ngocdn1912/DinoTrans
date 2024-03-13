using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.Entities
{
    public class ApplicationRole : IdentityRole<int>
    {
        public bool IsInternal { get; set; }
    }
}
