using DinoTrans.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.DTOs.Report
{
    public class DashboardForCarrier
    {
        public int GivenBids { get; set; }
        public int TendersInSelection { get; set; }
        public decimal SuccessRate { get; set; }
        public int TendersInExecution { get; set; }
        public int TendersCompleted { get; set; }
        public float TotalSuccessTenderMoney { get; set; }
        public float TotalMoneyForAdmin { get; set; }
        public List<TotalMoneyByCompany> TotalMoneyByShipperCompanies { get; set; }
        public ApplicationUser AdminInfo { get; set; }
    }
}
