using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.Entities
{
    public class Transportation
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public float CapacityTon { get; set; }
        public string LicensePlate { get; set; }
        public string Image { get; set; }
        public int CompanyCarrierId { get; set; }

        [ForeignKey("CompanyCarrierId")]
        public virtual Company? CompanyCarrier { get; set; }
    }
}
