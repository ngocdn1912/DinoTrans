using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DinoTrans.IdentityManagerServerAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class GetDataFromVnPayTransactionController : ControllerBase
    {
        private readonly IVnPayService _vpnPayService;
        public GetDataFromVnPayTransactionController(IVnPayService vnPayService) 
        {
            _vpnPayService = vnPayService;
        }
        [HttpGet]
        public async Task<IActionResult> VnPayReturn([FromQuery] VnPayReturnDTO model)
        {
            var result = await _vpnPayService.GetDataReturn(model);
            return Ok(result);
        }

        [HttpPost]
        public IActionResult TransacVNPay(int TenderId)
        {
            var result = _vpnPayService.TransacVNPay(TenderId);
            return Ok(result);
        }
    }
}
