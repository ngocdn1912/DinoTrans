using DinoTrans.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.DTOs.Report
{
    public class DashboardForShipper
    {
        public int ActiveTenderNumber { get;set; }
        public int ToAssignTenderNumber { get;set; }
        public int WithdrawTenderNumber { get;set; }
        public int InExecutionTenderNumber { get;set; }
        public int ConstructionMachineNumber { get;set; }
        public float PercentSubmitForTender { get;set; }
        public float PercentWithdrawTender { get;set; }
        public float TotalSuccessTenderMoney { get;set; }
        public int CompletedTenderNumber { get;set; }
        public ApplicationUser AdminInfo { get;set; }
        public List<TotalMoneyByCompany> StatisticByCompany { get;set; }
        public float TotalMoneyForAdmin { get; set; }
    }

    public class TotalMoneyByCompany
    {
        public string CompanyName { get; set; }
        public float TotalMoney { get; set; }
    }
}
