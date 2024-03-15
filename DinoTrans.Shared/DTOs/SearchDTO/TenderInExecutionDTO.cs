using DinoTrans.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.DTOs.SearchDTO
{
    public class TenderInExecutionDTO
    {
        public int TenderId { get; set; }
        public string TenderName { get; set; }
        public List<Entities.ContructionMachine> ConstructionMachines { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public DateTime PickUpDate { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string Status { get; set; }
        public List<TenderBid> Bids { get; set; }
        public int CompanyShipperId { get; set; }
        public string CompanyShipperName { get; set; }
        public int CompanyCarrierId { get; set; }
        public string CompanyCarrierName { get; set; }
        public float Price { get; set; }
    }
}
