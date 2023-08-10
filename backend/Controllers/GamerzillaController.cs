using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using backend.Models;
using backend.Filters;
using backend.Context;
using backend.Service;

namespace backend.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class GamerzillaController : ControllerBase
    {
        private readonly ILogger<GamerzillaController> _logger;
        private readonly SessionContext _sessionContext;
        private readonly GamerzillaService _gamerzillaService;
        private readonly UserService _userService;

        public GamerzillaController(ILogger<GamerzillaController> logger, SessionContext sessionContext, GamerzillaService gamerzillaService, UserService userService)
        {
            _logger = logger;
            _sessionContext = sessionContext;
            _gamerzillaService = gamerzillaService;
            _userService = userService;
        }

        private void ValidateClaim()
        {
                try
                {
                    _userService.ValidateClaim(User.Identity as ClaimsIdentity);
                }
                catch (System.Exception) { }
        }

        [Authorize]
        [Route("games")]
        [AllowAnonymous]
        public GameSummary GetGames1(string username, int pagesize = 20, int currentpage = 0)
        {
            int userId = 0;
            ValidateClaim();
            if (username != null)
            {
                userId = _userService.FindUser(username);
            }
            else
            {
                userId = _sessionContext.UserId;
            }
            return _gamerzillaService.GetPagedGames(userId, pagesize, currentpage);
        }

        [BasicAuth]
        [HttpPost]
        [Route("games")]
        public IList<GameShort> GetGames2()
        {
            return _gamerzillaService.GetGames(_sessionContext.UserId);
        }

        [Authorize]
        [Route("game")]
        [AllowAnonymous]
        public GameApi1 GetGame1(string game, string username)
        {
            int userId = 0;
            ValidateClaim();
            if (username != null)
            {
                userId = _userService.FindUser(username);
            }
            else
                userId = _sessionContext.UserId;
            return _gamerzillaService.GetGame(game, userId);
        }

        [BasicAuth]
        [HttpPost]
        [Route("game")]
        public GameApi1 GetGame2([FromForm] string game)
        {
            return _gamerzillaService.GetGame(game, _sessionContext.UserId);
        }

        [BasicAuth]
        [HttpPost]
        [Route("game/add")]
        public GameApi1 AddGame([FromForm] string game)
        {
            _logger.LogInformation("AddGame");
            GameApi1 gameInfo1 = JsonConvert.DeserializeObject<GameApi1>(game);
            return _gamerzillaService.AddGame(gameInfo1, _sessionContext.UserId);
        }

        [BasicAuth]
        [HttpPost]
        [Route("game/image")]
        public async Task<IActionResult> AddGameImage([FromForm] string game, [FromForm] IFormFile imagefile)
        {
            using (Stream s = imagefile.OpenReadStream())
            {
                if (_gamerzillaService.AddGameImage(game, s))
                    return Ok();
                else
                    return NotFound();
            }
        }

        [Route("game/image/show")]
        public async Task<IActionResult> ShowGameImage1(string game)
        {
            return await ShowGameImage(game);
        }

        [HttpPost]
        [Route("game/image/show")]
        public async Task<IActionResult> ShowGameImage2([FromForm] string game)
        {
            return await ShowGameImage(game);
        }

        private async Task<IActionResult> ShowGameImage(string game)
        {
            var s = _gamerzillaService.GetGameImage(game);
            if (s != null)
            {
                return new FileStreamResult(s, "image/png");
            }
            else
                return NotFound();
        }

        [BasicAuth]
        [HttpPost]
        [Route("trophy/image")]
        public async Task<IActionResult> AddTrophyImage([FromForm] string game, [FromForm] string trophy, [FromForm] IFormFile trueimagefile, [FromForm] IFormFile falseimagefile)
        {
            using (Stream s1 = trueimagefile.OpenReadStream())
            using (Stream s2 = falseimagefile.OpenReadStream())
            {
                if (_gamerzillaService.AddTrophyImage(game, trophy, s1, s2))
                    return Ok();
                else
                    return NotFound();
            }
        }

        [Route("trophy/image/show")]
        public async Task<IActionResult> ShowTrophyImage1(string game, string trophy, int achieved)
        {
            return await ShowTrophyImage(game, trophy, achieved);
        }

        [HttpPost]
        [Route("trophy/image/show")]
        public async Task<IActionResult> ShowTrophyImage2([FromForm] string game, [FromForm] string trophy, [FromForm] int achieved)
        {
            return await ShowTrophyImage(game, trophy, achieved);
        }

        private async Task<IActionResult> ShowTrophyImage(string game, string trophy, int achieved)
        {
            var s = _gamerzillaService.GetTrophyImage(game, trophy, achieved);
            if (s != null)
            {
                return new FileStreamResult(s, "image/png");
            }
            else
                return NotFound();
        }

        [BasicAuth]
        [HttpPost]
        [Route("trophy/set")]
        public async Task<IActionResult> SetTrophy([FromForm] string game, [FromForm] string trophy)
        {
            if (_gamerzillaService.SetUserStat(game, trophy, _sessionContext.UserId, true, 0))
                return Ok();
            else
                return NotFound();
        }

        [BasicAuth]
        [HttpPost]
        [Route("trophy/set/stat")]
        public async Task<IActionResult> SetTrophyStat([FromForm] string game, [FromForm] string trophy, [FromForm] int progress)
        {
            if (_gamerzillaService.SetUserStat(game, trophy, _sessionContext.UserId, false, progress))
                return Ok();
            else
                return NotFound();
        }
    }
}
