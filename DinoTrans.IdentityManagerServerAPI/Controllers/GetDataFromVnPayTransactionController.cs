using DinoTrans.IdentityManagerServerAPI.Services.Implements;
using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.Entities;
using DinoTrans.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DinoTrans.IdentityManagerServerAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class GetDataFromVnPayTransactionController : ControllerBase
    {
        private readonly IVnPayService _vpnPayService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ApplicationUser _currentUser;
        private readonly IUserService _userService;
        public GetDataFromVnPayTransactionController(IVnPayService vnPayService, IHttpContextAccessor httpContextAccessor,
            IUserService userService ) 
        {
            _vpnPayService = vnPayService;
            _httpContextAccessor = httpContextAccessor;
            _userService = userService;
            var claimsIdentity = _httpContextAccessor!.HttpContext!.User.Identity as ClaimsIdentity;
            var userIdParse = int.TryParse(claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value, out int userId);
            if (userIdParse)
            {
                var user = _userService.GetUserById(userId);
                if (user != null)
                    _currentUser = user.Data;
            }
        }
        [HttpPost]
        public async Task<IActionResult> VnPayReturn(Bill model)
        {
            var result = await _vpnPayService.GetDataReturn_ShipperToAdminDinoTrans(model, _currentUser);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> TransacVNPay([FromQuery] int TenderBidId)
        {
            var result = await _vpnPayService.TransacVNPay(TenderBidId, _currentUser);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> GetDataReturn_AdminDinoTransToCarrier(Bill model)
        {
            var result = await _vpnPayService.GetDataReturn_AdminDinoTransToCarrier(model);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> TransacVNPay_FromAdmin([FromQuery] int TenderBidId)
        {
            var result = await _vpnPayService.TransacVNPay_FromAdmin(TenderBidId);
            return Ok(result);
        }
    }
}
