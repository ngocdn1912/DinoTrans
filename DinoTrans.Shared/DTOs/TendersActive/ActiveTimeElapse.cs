using DinoTrans.Shared.Entities;
using Timer = System.Timers.Timer;

namespace DinoTrans.Shared.DTOs.TendersActive
{
    public class ActiveTimeElapse
    {
        public int TenderId { get; set; }
        public string TenderName { get; set; }
        public List<Entities.ContructionMachine> ConstructionMachines { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public DateTime PickUpDate { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string Status { get; set; }
        public double TimeRemaining { get; set; }
        public List<TenderBid> Bids { get; set; }
        public int CompanyShipperId { get; set; }
        public string CompanyShipperName { get; set; }
        public Timer Timer { get; set; }
        public string TimeLeft { get; set; }
    }
}
