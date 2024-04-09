using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.DTOs.Report
{
    public class DashboardForCarrier
    {
        public int GivenBIds { get; set; }
        public int TendersInSelection { get; set; }
        public float SuccessRate { get; set; }
        public int TendersInExecution { get; set; }
        public int TendersCompleted { get; set; }
        public float TotalSuccessTenderMoney { get; set; }
        public int TotalMoneyForAdmin { get; set; }
    }
}
