using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.DTOs
{
    public class WithdrawTenderDTO
    {
        public int TenderID { get; set; }
        [Required]
        public required string WithdrawReason { get; set; }
    }
}
