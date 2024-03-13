using DinoTrans.Shared.Contracts;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace DinoTrans.Shared.Entities
{
    public class Tender
    {
        public int Id { get; set; }
        public string Name { get; set; }
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
        public float? FinalPrice { get; set; }
        public bool IsShipperComfirm { get; set; } = false;
        public bool IsCarrierComfirm { get; set; } = false;
        [ForeignKey("CompanyShipperId")]
        public virtual Company? CompanyShipper { get; set; }
        [ForeignKey("CompanyCarrierId")]
        public virtual Company? CompanyCarrier { get; set; }
        [NotMapped]
        public List<Dictionary<string, string>> DocumentDeserializeJson
        {
            get
            {
                if (!string.IsNullOrEmpty(Documentations))
                {
                    return JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(Documentations);
                }

                return new List<Dictionary<string, string>>();
            }
            set => Documentations = JsonConvert.SerializeObject(value);
        }
        public string? WithdrawReason { get; set; }

    }
}
