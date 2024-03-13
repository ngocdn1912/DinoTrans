using DinoTrans.Shared.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.DTOs.UserResponse
{
    public class UserInfoResponseDTO
    {
        public UserInfo UserInfo { get; set; }
        public CompanyInfo Company { get; set; }
    }

    public class UserInfo()
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Address { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }    

    public class CompanyInfo()
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyEmail { get; set; } = string.Empty;
        public string CompanyPhoneNumber { get; set; } = string.Empty;
        public CompanyRoleEnum CompanyRole { get; set; }
        public string CompanyRoleName { get; set; } = string.Empty;
        public string CompanyAddress { get; set; } = string.Empty;
    }

}
