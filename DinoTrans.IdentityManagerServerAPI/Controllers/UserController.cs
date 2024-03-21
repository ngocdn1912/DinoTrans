﻿using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.Entities;
using DinoTrans.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Principal;

namespace DinoTrans.IdentityManagerServerAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ApplicationUser _currentUser;
        public UserController(IUserService userService, IHttpContextAccessor contextAccessor)
        {
            _userService = userService;
            _contextAccessor = contextAccessor;
            var context = _contextAccessor.HttpContext.User.Identity as ClaimsIdentity;
            if (context != null)
            {
                var userIdParse = int.TryParse(context!.FindFirst(ClaimTypes.NameIdentifier).Value, out int userId);
                if (userIdParse)
                {
                    var user = _userService.GetUserById(userId);
                    if (user != null)
                        _currentUser = user.Data;
                }
            }
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

        [HttpGet]
        public async Task<IActionResult> GetUserById([FromQuery] int UserId)
        {
            var response = _userService.GetUserById(UserId);
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccountForUserOfCompany(CreateAccountForUserOfCompany dto)
        {
            var response = await _userService.CreateAccountForUserOfCompany(dto, _currentUser);
            return Ok(response);
        }
    }
}
