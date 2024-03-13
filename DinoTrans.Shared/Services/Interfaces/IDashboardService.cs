using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.DTOs.Report;
using DinoTrans.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<ResponseModel<DashboardForShipper>> GetDashBoardForShipper(ApplicationUser _currentUser);
    }
}
