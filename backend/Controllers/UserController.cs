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
using Microsoft.AspNetCore.Http.HttpResults;

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
        [HttpGet]
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

        [Authorize]
        [HttpPost("promote")]
        public async Task<IActionResult> PromoteUser(string username)
        {
            await _userService.ValidateClaim(User.Identity as ClaimsIdentity);
            var currentUser = _userService.GetCurrentUser();

            if (currentUser == null || !currentUser.admin)
            {
                return BadRequest("You do not have permission to promote users.");
            }

            
            string result = await _userService.PromoteToAdminAsync(username);

            if (result == "Success")
            {
                return Ok($"User {username} successfully promoted to Admin.");
            }

            
            return BadRequest(result);
        }


        [HttpPost("demote")]
        [Authorize] 
        public async Task<IActionResult> Demote([FromQuery] string username)
        {
            await _userService.ValidateClaim(User.Identity as ClaimsIdentity);
            var currentUser = _userService.GetCurrentUser();

            if (currentUser == null || !currentUser.admin)
            {
                return BadRequest("You do not have permission to promote users.");
            }

            
            var result = await _userService.DemoteAdminAsync(username, currentUser.userName);

            
            if (result == "Success")
            {
                return Ok($"User {username} was demoted to a regular user.");
            }

            
            
            return BadRequest(result);
        }

        [Authorize]
        [HttpPost("delete")]
        public async Task<IActionResult> DeleteUser(string username)
        {
            
            await _userService.ValidateClaim(User.Identity as ClaimsIdentity);
            var currentUser = _userService.GetCurrentUser();

            if (currentUser == null) return Unauthorized();

            
            
            if (string.Equals(currentUser.userName, username, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("You cannot delete your own account.");
            }

            
            
            bool success = await _userService.DeleteUserAsync(username);

            if (success)
            {
                return Ok();
            }

            return BadRequest("User not found or could not be deleted.");
        }


        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginInfo login)
        {
            
            if (!_userService.IsValidUser(login.username, login.password))
                return BadRequest("Invalid username or password.");

            
            var userDto = await _userService.GetUserByUsernameAsync(login.username);
            if (userDto == null) return BadRequest("Invalid username or password.");

            
            if (!userDto.approved)
            {
                
                
                return Ok(userDto);
            }

            
            var claimsIdentity = new ClaimsIdentity(new[]
            {
        new Claim(ClaimTypes.Name, userDto.userName),
    }, "Cookies");

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            await Request.HttpContext.SignInAsync("Cookies", claimsPrincipal);

            
            await _userService.ValidateClaim(claimsIdentity);

            return Ok(userDto);
        }

        [Route("logout")]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await Request.HttpContext.SignOutAsync();
            return Ok();
        }

        [HttpGet("verify")]
        public async Task<IActionResult> Verify(string token)
        {
            if (string.IsNullOrEmpty(token)) return BadRequest("Token required");

            try
            {
                var result = await _userService.ProcessTokenAsync(token);
                return Ok(result); 
            }
            catch (TokenException ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] LoginInfo login)
        {
            
            if ((_options.Allow == false) || string.IsNullOrEmpty(login.username))
                return BadRequest("Registration is currently disabled.");

            try
            {
                
                
                var u = await _userService.RegisterUser(login.username, login.password, login.email);

                
                if (u.Approved)
                {
                    var claimsIdentity = new ClaimsIdentity(new[]
                    {
                new Claim(ClaimTypes.Name, u.UserName),
            }, "Cookies");

                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                    await Request.HttpContext.SignInAsync("Cookies", claimsPrincipal);
                    await _userService.ValidateClaim(claimsIdentity);
                }

                
                u.Password = "";
                u.VerificationToken = "";
                return Ok(u);
            }
            catch (TokenException ex)
            {
                
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An internal error occurred during registration.");
            }
        }



        [HttpGet]
        [Route("canregister")]
        public IActionResult CanRegister()
        {
            return Ok(_options.Allow);
        }

        [Authorize]
        [HttpGet]
        [Route("whoami")]
        public async Task<IActionResult> WhoAmI()
        {
            await _userService.ValidateClaim(User.Identity as ClaimsIdentity);
            return Ok(_userService.GetCurrentUser());
        }

        [Authorize]
        [Route("approve")]
        [HttpPost]
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
        [HttpPost]
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
        public string email { get; set; }
    }
}
