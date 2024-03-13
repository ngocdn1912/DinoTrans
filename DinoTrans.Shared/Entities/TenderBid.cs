using FluentNHibernate.Conventions.Inspections;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.Entities
{
    public class TenderBid
    {
        public int Id { get; set; }
        public int TenderId { get; set; }
        public int CompanyCarrierId { get; set; }
        public float TransportPrice { get; set; }
        public float? ShipperFee { get; set; }
        public float? CarrierFee { get; set; }
        public bool IsSelected { get; set; }

        [ForeignKey("TenderId")]
        public Tender? Tender { get; set; }

        [ForeignKey("CompanyCarrierId")]
        public virtual Company? CompanyCarrier { get; set; }
    }
}
