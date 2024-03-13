using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.Entities
{
    public class TenderContructionMachine
    {
        public int Id { get; set; }
        public int TenderId { get; set; }
        public int ContructionMachineId { get; set; }
        [ForeignKey("TenderId")]
        public virtual Tender? Tender { get; set; }
        [ForeignKey("ContructionMachineId")]
        public virtual ContructionMachine? ContructionMachine { get;set; }
    }
}
