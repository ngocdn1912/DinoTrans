using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DinoTrans.Shared.DTOs.ServiceResponses;

namespace DinoTrans.Shared.Services.Interfaces
{
    public interface IVnPayService
    {
        public Task<GeneralResponse> TransacVNPay(int TenderBidId, ApplicationUser _currentUser);
        public Task<GeneralResponse> GetDataReturn_ShipperToAdminDinoTrans(Bill dto, ApplicationUser _currentUser);
        public Task<GeneralResponse> TransacVNPay_FromAdmin(int TenderBidId);
        public Task<GeneralResponse> GetDataReturn_AdminDinoTransToCarrier(Bill dto);
    }
}
