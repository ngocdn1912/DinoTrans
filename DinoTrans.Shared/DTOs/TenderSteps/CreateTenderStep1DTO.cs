using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.DTOs.TenderSteps
{
    public class CreateTenderStep1DTO
    {
        public string TenderName { get; set; }
        public int CompanyShipperId { get; set; }
        public DateTime TenderStartDate { get; set; }
        public DateTime TenderEndDate { get; set; }
    }
}
