using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.DTOs.SearchDTO
{
    public class SearchBill: SearchModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public BillTypeEnum? BillType { get; set; }
    }
}
