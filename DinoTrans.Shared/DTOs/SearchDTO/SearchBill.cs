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
        public BillTypeSearchModel BillType { get; set; }
        public AmountTypeSearchModel AmountType { get; set; }
    }

    public enum AmountTypeSearchModel
    {
        All,
        LessThan5M,
        From5MTo10M,
        MoreThan10M
    }

    public enum BillTypeSearchModel
    {
        /// <summary>
        /// tất cả các bill
        /// </summary>
        All,
        /// <summary>
        /// Shipper cho Admin
        /// </summary>
        ShipperToAdmin,
        /// <summary>
        /// Admin cho carrier
        /// </summary>
        AdminToCarrier,
    }
}
