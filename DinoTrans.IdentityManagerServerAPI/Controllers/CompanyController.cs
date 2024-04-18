using DinoTrans.IdentityManagerServerAPI.Services.Implements;
using DinoTrans.Shared.DTOs.Company;
using DinoTrans.Shared.DTOs.ContructionMachine;
using DinoTrans.Shared.Entities;
using DinoTrans.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.Design;
using System.Security.Claims;

namespace DinoTrans.IdentityManagerServerAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyService _companyService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ApplicationUser _currentUser;
        private readonly IUserService _userService;

        public CompanyController(ICompanyService companyService, IHttpContextAccessor httpContextAccessor, IUserService userService)
        {
            _httpContextAccessor = httpContextAccessor;
            _companyService = companyService;
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

        [HttpGet]
        public async Task<IActionResult> GetCompanyByCurrentUserId()
        {
            var result = await _companyService.GetCompanyByCurrentUserId(_currentUser);
            return Ok(result);
        }
        [HttpPut]
        public async Task<IActionResult> UpdateCompanyInforByAdminOfCompany(UpdateCompanyDTO dto)
        {
            var result = await _companyService.UpdateCompanyInforByAdminOfCompany(dto);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetCompanyByCompanyId([FromQuery] int CompanyId)
        {
            var result = await _companyService.GetCompanyByCompanyId(CompanyId);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCompaniesByAdmin()
        {
            var result = await _companyService.GetAllCompaniesByAdmin();
            return Ok(result);
        }
    }
}
