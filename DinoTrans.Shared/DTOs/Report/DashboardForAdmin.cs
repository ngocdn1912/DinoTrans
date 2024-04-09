using DinoTrans.Shared.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.DTOs.Report
{
    public class DashboardForAdmin
    {
        public List<CompanyReport> ListCompanyReports { get; set; }
    }

    public class CompanyReport
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public int UserCount { get; set; }
        public string AdminName { get; set; }
        public int MachineCount { get; set; }
        public float Amount { get; set; }
        public CompanyRoleEnum CompanyRole { get; set; }
    }
}
