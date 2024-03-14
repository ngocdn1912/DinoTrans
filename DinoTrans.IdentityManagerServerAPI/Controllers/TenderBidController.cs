using DinoTrans.IdentityManagerServerAPI.Services.Implements;
using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.DTOs.TenderSteps;
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
    public class TenderBidController : ControllerBase
    {
        private readonly ITenderBidService _tenderBidService;
        private readonly IHttpContextAccessor _contextAccessor;
        private ApplicationUser _currentUser;
        private readonly IUserService _userService;
        public TenderBidController(
            ITenderBidService tenderBidService,
            IHttpContextAccessor httpContextAccessor,
            IUserService userService)
        {
            _tenderBidService = tenderBidService;
            _contextAccessor = httpContextAccessor;
            _userService = userService;
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            var claimsIdentity = _contextAccessor!.HttpContext!.User.Identity as ClaimsIdentity;
            var userIdParse = int.TryParse(claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value, out int userId);
            if (userIdParse)
            {
                var user = await _userService.GetUserById(userId);
                if (user != null)
                    _currentUser = user.Data;
            }
        }

        [HttpPost]
        public async Task<IActionResult> SubmitTenderBid(TenderBidDTO dto)
        {
            var result = await _tenderBidService.SubmitTenderBid(dto, _currentUser);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> ChooseTenderBid([FromBody] int TenderBidId)
        {
            var result = await _tenderBidService.ChooseTenderBid(TenderBidId, _currentUser);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetTenderBidsByTenderId([FromQuery] int TenderId)
        {
            var result = await _tenderBidService.GetTenderBidsByTenderId(TenderId);
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateTenderBid(UpdateTenderBidDTO dto)
        {
            var result = await _tenderBidService.UpdateTenderBid(dto);
            return Ok(result);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteTenderBid([FromQuery] int TenderBidId)
        {
            var result = await _tenderBidService.DeleteTenderBid(TenderBidId);
            return Ok(result);
        }
    }
}
