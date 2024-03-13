using DinoTrans.Shared.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.Entities
{
    public class Company
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public CompanyRoleEnum Role { get; set; }
        public string Address { get; set; } = string.Empty;
        public float ShipperFeePercentage { get; set; }
        public float CarrierFeePercentage { get; set; }
        public bool? IsAdminConfirm { get; set; }
    }
}
