using DinoTrans.Shared.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.DTOs
{
    public class UserDTO
    {
        //new user
        [Required]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        public string LastName { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = string.Empty;
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = string.Empty;
        [Required]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Address { get; set; } = string.Empty;
        [Required]
        public string Role { get; set; }

        //new company
        [Required]
        public string CompanyName { get; set; } = string.Empty;
        [DataType(DataType.EmailAddress)]
        [Required]
        public string CompanyEmail { get; set; } = string.Empty;
        [Required]
        public string CompanyPhoneNumber { get; set; } = string.Empty;
        [Required]
        public CompanyRoleEnum CompanyRole { get; set; }
        [Required]
        public string CompanyAddress { get; set; } = string.Empty;

/*        //new Location
        [Required]
        public string LocationName { get; set; } = string.Empty;
*/    }
}
