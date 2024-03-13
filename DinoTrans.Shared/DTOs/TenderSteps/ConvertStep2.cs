using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.DTOs.TenderSteps
{
    public class ConvertStep2
    {
        public int TenderId { get; set; }
        public DateOnly PickUpDate { get; set; }
        public DateOnly DeliveryDate { get; set; }
        public TimeOnly PickUpTime { get; set; }
        public TimeOnly DeliveryTime { get; set; }
        public List<int> ConstructionMachineIds { get; set; }
        public string PickUpAddress { get; set; }
        public string DeliveryAddress { get; set; }
        public string ContactAtPickUpAddress { get; set; }
        public string ContactAtDeliveryAddress { get; set; }
        public string Notes { get; set; }
        public string Documentations { get; set; }
    }
}
