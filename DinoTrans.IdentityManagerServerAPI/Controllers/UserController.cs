using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DinoTrans.IdentityManagerServerAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> Register (UserDTO userDTO)
        {
            var response = await _userService.CreateAccount(userDTO);
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> Login (LoginDTO loginDTO)
        {
            var response = await _userService.LoginAccount(loginDTO);
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO changePasswordDTO)
        {
            var response = await _userService.ChangeUserPassword(changePasswordDTO);
            return Ok(response);
        }

        [HttpGet]
        [Authorize(Roles = "CompanyAdministrator")]
        public async Task<IActionResult> GetAllUserInfo([FromQuery] GetAllUserInfoDTO userInfo)
        {
            var response = await _userService.GetAllUserInfo(userInfo);
            return Ok(response);
        }
        [HttpPut]
        [Authorize(Roles = "DinoTransAdmin")]
        public async Task<IActionResult> UpdateAdminConfirm()
        {
            var response = await _userService.UpdateIsAdminConfirm();
            return Ok(response);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetCompanyRole([FromQuery] int companyId)
        {
            var response = await _userService.GetCompanyRole(companyId);
            return Ok(response);
        }

    }
}
