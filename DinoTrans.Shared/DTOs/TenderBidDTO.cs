using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.DTOs
{
    public class TenderBidDTO
    {
        public int TenderId { get; set; }
        public int CompanyCarrierId { get; set; }
        public float TransportPrice { get; set; }
        public float? ShipperFee { get; set; }
        public float? CarrierFee { get; set; }
        public bool IsSelected { get; set; } = false;
    }
}
