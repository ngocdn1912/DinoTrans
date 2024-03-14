using DinoTrans.IdentityManagerServerAPI.Services.Implements;
using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.Entities;
using DinoTrans.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DinoTrans.IdentityManagerServerAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly IHttpContextAccessor _contextAccessor;
        private ApplicationUser _currentUser;
        private readonly IUserService _userService;
        public DashboardController(IDashboardService service,
            IHttpContextAccessor httpContextAccessor,
            IUserService userService) 
        {
            _dashboardService = service;
            _contextAccessor = httpContextAccessor;
            _userService = userService;
            var claimsIdentity = _contextAccessor!.HttpContext!.User.Identity as ClaimsIdentity;
            var userIdParse = int.TryParse(claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value, out int userId);
            if (userIdParse)
            {
                var user = _userService.GetUserById(userId);
                if (user != null)
                    _currentUser = user.Data;
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDashBoardForShipper()
        {
            var result = await _dashboardService.GetDashBoardForShipper(_currentUser);
            return Ok(result);
        }
    }
}
