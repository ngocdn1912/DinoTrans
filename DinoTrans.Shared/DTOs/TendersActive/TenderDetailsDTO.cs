using DinoTrans.Shared.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.DTOs.TendersActive
{
    public class TenderDetailsDTO
    {
        public int TenderId { get; set; }
        public string TenderName { get; set; }
        public TenderStatuses TenderStatus { get; set; }
        public int CompanyShipperId { get; set; }
        public int? CompanyCarrierId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? PickUpDate { get; set; }
        public DateTime? DeiliverDate { get; set; }
        public string? PickUpAddress { get; set; }
        public string? PickUpContact { get; set; }
        public string? DeliveryAddress { get; set; }
        public string? DeliveryContact { get; set; }
        public string? Notes { get; set; }
        public string? Documentations { get; set; }
        public List<ConstructionMachinesForTendersDTO> ConstructionMachines { get; set; }
        public bool IsShipperConfirm { get; set; }
        public bool IsCarrierConfirm { get; set; }
    }

    public class ConstructionMachinesForTendersDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public string SerialNumber { get; set; }
        public int CompanyShipperId { get; set; }
        public string? Image { get; set; }
        public float Length { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float Weight { get; set; }
    }
}
