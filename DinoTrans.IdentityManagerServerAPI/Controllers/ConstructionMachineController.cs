using DinoTrans.IdentityManagerServerAPI.Services.Implements;
using DinoTrans.Shared.DTOs.ContructionMachine;
using DinoTrans.Shared.DTOs.SearchDTO;
using DinoTrans.Shared.Entities;
using DinoTrans.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DinoTrans.IdentityManagerServerAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [Authorize]
    [ApiController]
    public class ConstructionMachineController : ControllerBase
    {
        private readonly IConstructionMachineService _constructionMachineService;
        private readonly ICompanyService _companyService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ApplicationUser _currentUser;
        private readonly IUserService _userService;
        public ConstructionMachineController(IConstructionMachineService constructionMachineService,
            IHttpContextAccessor httpContextAccessor,
            IUserService userService)
        {
            _constructionMachineService = constructionMachineService;
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
        public async Task<IActionResult> CreateContructionMachine(CreateContructionMachineDTO dto)
        {
            var result =  await _constructionMachineService.CreateContructionMachine(dto);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> SearchConstructionMachineForTender(SearchLoadForTenderDTO dto)
        {
            var result = await _constructionMachineService.SearchConstructionMachineForTender(dto);
            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> SearchConstructionMachine(SearchLoadDTO dto)
        {
            var result = await _constructionMachineService.SearchConstructionMachine(dto);
            return Ok(result);
        }


        [HttpGet]
        public async Task<IActionResult> GetMachinesForTenderOverviewByIds([FromQuery] int TenderId)
        {
            var result = await _constructionMachineService.GetMachinesForTenderOverviewByIds(TenderId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> GetMachinesByCurrentShipperId(SearchLoadDTO dto)
        {
            var result = await _constructionMachineService.GetMachinesByCurrentShipperId(dto, _currentUser);
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> EditConstructionMachine(EditConstructionMachineDTO dto)
        {
            var result = await _constructionMachineService.EditConstructionMachine(dto);
            return Ok(result);
        }
    }
}
