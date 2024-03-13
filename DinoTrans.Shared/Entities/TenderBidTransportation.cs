using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.Entities
{
    public class TenderBidTransportation
    {
        public int Id { get; set; }
        public int TenderBidId { get; set; }
        public int TransportationId { get; set; }
        public string TransportationNotes { get; set; }

        [ForeignKey("TenderBidId")]
        public TenderBid TenderBid { get; set; }

        [ForeignKey("TransportationId")]
        public Transportation Transportation { get; set; }

    }
}
