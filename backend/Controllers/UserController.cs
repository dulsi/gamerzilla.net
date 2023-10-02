using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using backend.Dto;
using backend.Models;
using backend.Filters;
using backend.Context;
using backend.Service;
using backend.Settings;

namespace backend.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly SessionContext _sessionContext;
        private readonly UserService _userService;
        private readonly RegistrationOptions _options;

        public UserController(ILogger<UserController> logger, SessionContext sessionContext, UserService userService, IOptions<RegistrationOptions> options)
        {
            _logger = logger;
            _sessionContext = sessionContext;
            _userService = userService;
            _options = options.Value;
        }

        [Authorize]
        [AllowAnonymous]
        public async Task<IEnumerable<UserInfoDto>> GetUsers()
        {
            bool admin = false;
            try
            {
                await _userService.ValidateClaim(User.Identity as ClaimsIdentity);
                admin = _userService.GetCurrentUser().admin;
            }
            catch (System.Exception) { }
            return _userService.GetUsers(admin);
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginInfo login)
        {
            if (!_userService.IsValidUser(login.username, login.password))
                return BadRequest();

            var claimsIdentity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, login.username),
            }, "Cookies");

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            await Request.HttpContext.SignInAsync("Cookies", claimsPrincipal);
            await _userService.ValidateClaim(claimsIdentity);
            return Ok(_userService.GetCurrentUser());
        }

        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            await Request.HttpContext.SignOutAsync();
            return Ok();
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] LoginInfo login)
        {
            if ((_options.Allow == false) || (login.username == ""))
                return BadRequest();
            var u = await _userService.RegisterUser(login.username, login.password);
            return Ok(u);
        }

        [HttpGet]
        [Route("canregister")]
        public IActionResult CanRegister()
        {
            return Ok(_options.Allow);
        }

        [Authorize]
        [Route("whoami")]
        public async Task<IActionResult> WhoAmI()
        {
            await _userService.ValidateClaim(User.Identity as ClaimsIdentity);
            return Ok(_userService.GetCurrentUser());
        }

        [Authorize]
        [Route("approve")]
        public async Task<IActionResult> Approve(string username)
        {
            bool admin = false;
            try
            {
                await _userService.ValidateClaim(User.Identity as ClaimsIdentity);
                admin = _userService.GetCurrentUser().admin;
            }
            catch (System.Exception) { }
            if (!admin)
                return BadRequest();
            if (_userService.Approve(username))
                return Ok(true);
            else
                return BadRequest();
        }

        [Authorize]
        [Route("visible")]
        public async Task<IActionResult> Visible(string username, int val)
        {
            bool admin = false;
            string currentname = "";
            try
            {
                await _userService.ValidateClaim(User.Identity as ClaimsIdentity);
                var user = _userService.GetCurrentUser();
                admin = user.admin;
                currentname = user.userName;
            }
            catch (System.Exception) { }
            if (!admin && (username != currentname))
                return BadRequest();
            if (_userService.Visible(username, val))
                return Ok(true);
            else
                return BadRequest();
        }
    }

    public class LoginInfo
    {
        public string username { get; set; }
        public string password { get; set; }
    }
}
