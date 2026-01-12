using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using backend.Service;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using backend.Models;
using System; 

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly UserService _userService;

        public AccountController(UserService userService)
        {
            _userService = userService;
        }

        
        private async Task ValidateClaim()
        {
            try
            {
                await _userService.ValidateClaim(User.Identity as ClaimsIdentity);
            }
            catch { /* Ignore errors, GetCurrentUser will handle nulls */ }
        }

        [HttpPost("request-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RequestToken([FromBody] TokenRequest req)
        {
            
            var result = await _userService.RequestTokenAsync(req.Identifier);

            if (result == "Wait") return BadRequest("Wait");
            return Ok();
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
        {
            try
            {
                
                var result = await _userService.ProcessTokenAsync(req.Token, req.NewPassword);

                
                return Ok(result.Message);
            }
            catch (TokenException ex)
            {
                
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                
                return StatusCode(500, "An internal error occurred.");
            }
        }

        //[HttpPost("request-reset")]
        //public async Task<IActionResult> RequestReset([FromBody] string emailOrUsername)
        //{
        
        
        
        //}

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest req)
        {
            await ValidateClaim();

            var user = _userService.GetCurrentUser();
            if (user == null) return Unauthorized();

            try
            {
                var result = await _userService.ChangePasswordAsync(user.id, req.OldPassword, req.NewPassword);
                return Ok(result); 
            }
            catch (TokenException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("update-email")]
        public async Task<IActionResult> UpdateEmail([FromBody] UpdateEmailRequest req)
        {
            await ValidateClaim();

            var user = _userService.GetCurrentUser();
            if (user == null) return Unauthorized();

            try
            {
                var result = await _userService.RequestEmailUpdateAsync(user.id, req.NewEmail);
                return Ok(result); 
            }
            catch (TokenException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public class TokenRequest { public string Identifier { get; set; } }
        public class ChangePasswordRequest { public string OldPassword { get; set; } public string NewPassword { get; set; } }
        public class UpdateEmailRequest { public string NewEmail { get; set; } }

        public class ResetPasswordRequest
        {
            public string Token { get; set; }
            public string NewPassword { get; set; }
        }

        
        public class ForgotPasswordRequest
        {
            public string EmailOrUsername { get; set; }
        }
    }
}