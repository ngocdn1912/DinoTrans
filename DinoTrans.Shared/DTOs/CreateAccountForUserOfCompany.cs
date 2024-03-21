using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.DTOs
{
    public class CreateAccountForUserOfCompany
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName => $"{FirstName.Normalize()}{LastName.Normalize()}{new Guid()}@gmail.com";
        public string Email => UserName;
        public string Password = "111501Abc#";
    }
}
