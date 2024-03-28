using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.Services.Interfaces
{
    public interface IBillService
    {
        public Task<ResponseModel<List<Bill>>> GetAllBills(ApplicationUser _currentUser);
        public Task<ResponseModel<Bill>> GetBillDetail(int BillId);
    }
}
