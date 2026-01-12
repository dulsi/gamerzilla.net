using Microsoft.AspNetCore.Authorization;
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfigController : Controller
    {

        private readonly IConfiguration _configuration;

        public ConfigController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("options")]
        [AllowAnonymous]
        public IActionResult GetServerOptions()
        {

            string mode = _configuration["AppMode"] ?? "Standalone";
            bool isHubzilla = mode.Equals("Hubzilla", StringComparison.OrdinalIgnoreCase);


            bool allowRegistration = _configuration.GetValue<bool>("RegistrationOptions:Allow");


            bool emailEnabled = _configuration.GetValue<bool>("EmailSettings:Enabled");

            return Ok(new
            {
                canRegister = allowRegistration,


                allowPasswordChange = !isHubzilla,
                allowEmailChange = !isHubzilla,

                emailEnabled = emailEnabled
            });
        }

    }

}