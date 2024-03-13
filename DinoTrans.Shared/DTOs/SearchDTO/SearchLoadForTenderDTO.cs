using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.DTOs.SearchDTO
{
    public class SearchLoadForTenderDTO:SearchModel
    {
        public DateTime PickUpDate { get; set; }
        public DateTime DeliveryDate { get; set; }
        public int TenderId { get; set; }
    }
}
