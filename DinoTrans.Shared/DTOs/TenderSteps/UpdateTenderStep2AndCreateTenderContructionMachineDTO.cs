using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.DTOs.TenderSteps
{
    public class UpdateTenderStep2AndCreateTenderContructionMachineDTO
    {
        public int TenderId { get; set; }
        public DateTime PickUpDateAndTime { get; set; }
        public DateTime DeliveryDateAndTime { get; set; }
        public string PickUpAddress { get; set; }
        public string DeliveryAddress { get; set; }
        public string ContactAtPickUpAddress { get; set; }
        public string ContactAtDeliveryAddress { get; set; }
        public string Notes { get; set; }
        public List<string>? Documentations { get; set; }
        public List<int> ConstructionMachineIds { get; set; }

    }
}
