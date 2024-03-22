using DinoTrans.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.Services.Interfaces
{
    public interface IVnPayService
    {
        public string TransacVNPay(int Tenderid);
        public Task<VnPayReturnDTO> GetDataReturn(VnPayReturnDTO dto);
    }
}
