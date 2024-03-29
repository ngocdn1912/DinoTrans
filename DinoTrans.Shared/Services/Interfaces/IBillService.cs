using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.DTOs.SearchDTO;
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
        public Task<ResponseModel<List<BillDTO>>> GetAllBills(SearchBill model, ApplicationUser _currentUser);
        public Task<ResponseModel<BillDTO>> GetBillDetail(int BillId);
    }
}
