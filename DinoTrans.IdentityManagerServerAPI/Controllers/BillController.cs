using DinoTrans.IdentityManagerServerAPI.Services.Implements;
using DinoTrans.Shared.DTOs.SearchDTO;
using DinoTrans.Shared.Entities;
using DinoTrans.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DinoTrans.IdentityManagerServerAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BillController : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ApplicationUser _currentUser;
        private readonly IUserService _userService;
        private readonly IBillService _billService;

        public BillController(IHttpContextAccessor httpContextAccessor, IUserService userService, IBillService billService)
        {
            _httpContextAccessor = httpContextAccessor;
            _billService = billService;
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
        public async Task<IActionResult> GetAllBills(SearchBill modl)
        {
            var result = await _billService.GetAllBills(modl, _currentUser);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetBillDetail(int BillId)
        {
            var result = await _billService.GetBillDetail(BillId);
            return Ok(result);
        }
    }
}
