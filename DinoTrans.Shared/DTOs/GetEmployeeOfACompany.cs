﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.DTOs
{
    public class GetEmployeeOfACompany
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get;set; }
        public string Address { get; set; }
        public string Email { get; set; }
    }
}
