using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.DTOs
{
    public class BillDTO
    {
        public int BillId { get; set; }
        public int TenderId {  get; set; }
        public string Name { get; set; }
        public float? Amount { get; set; }
        public BillTypeEnum? BillType { get; set; }
        public string? BankCode { get;set; }
        public string? BankTransNo { get; set; }
        public string? TransactionNo { get; set; }
        public string? CardType { get; set; }
        public string? OrderInfo { get; set; }
        public DateTime? PayDate { get; set; }
        public int CompanyShipperId { get; set; }
        public int CompanyCarrierId { get; set; }
    }
}
