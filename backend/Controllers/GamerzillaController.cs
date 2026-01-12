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

        private async Task ValidateClaim()
        {
                try
                {
                    await _userService.ValidateClaim(User.Identity as ClaimsIdentity);
                }
                catch (System.Exception) { }
        }

        [Authorize]
        [HttpGet]
        [Route("games")]
        [AllowAnonymous]
        public async Task<GameSummary> GetGames1(string username, int pagesize = 10, int currentpage = 0)
        {
            int userId = 0;
            await ValidateClaim();
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
        [HttpGet]
        [Route("game")]
        [AllowAnonymous]
        public async Task<GameApi1> GetGame1(string game, string username)
        {
            int userId = 0;
            await ValidateClaim();
            if (username != null)
            {
                userId = _userService.FindUser(username);
            }
            else
                userId = _sessionContext.UserId;
            return _gamerzillaService.GetGame(game, userId);
        }

        [Authorize]
        [HttpGet("game/list/owned")]
        
        public async Task<IActionResult> GetMyGames([FromQuery] int pagesize = 50, [FromQuery] int currentpage = 0)
        {
            await ValidateClaim();
            var user = _userService.GetCurrentUser();
            if (user == null) return Unauthorized();

            var result = await _gamerzillaService.GetOwnedGamesAsync(user.id, user.admin, pagesize, currentpage);

            return Ok(result);
        }

        
        [Authorize]
        [HttpPost("game/transfer")]
        public async Task<IActionResult> TransferGame([FromBody] TransferRequest req)
        {
            
            
            await ValidateClaim(); 
            var currentUser = _userService.GetCurrentUser();
            if (currentUser == null) return Unauthorized();

            
            int targetUserId = _userService.FindUser(req.NewOwnerUsername);
            if (targetUserId == 0)
            {
                return BadRequest($"User '{req.NewOwnerUsername}' not found or not eligible.");
            }

            
            string result = await _gamerzillaService.TransferOwnershipAsync(
                currentUser.id,
                currentUser.admin,
                req.GameId,
                targetUserId
            );

            
            if (result == "Success")
            {
                return Ok($"Transferred ownership to {req.NewOwnerUsername}");
            }

            if (result == $"Verification email sent to {req.NewOwnerUsername}")
            {
                return Ok(result);
            }

            if (result == "Unauthorized") return Forbid();
            if (result == "Game not found") return NotFound("Game not found.");

            
            return BadRequest(result);
        }

        [BasicAuth]
        [HttpPost]
        [Route("game")]
        public GameApi1 GetGame2([FromForm] string game)
        {
            return _gamerzillaService.GetGame(game, _sessionContext.UserId);
        }

        [BasicAuth]
        [HttpPost("game/add")]
        public async Task<IActionResult> AddGame([FromForm] string game)
        {
            _logger.LogInformation("AddGame");

            
            await ValidateClaim();
            var user = _userService.GetCurrentUser();
            if (user == null) return Unauthorized();

            GameApi1 gameInfo1 = JsonConvert.DeserializeObject<GameApi1>(game);

            
            var result = await _gamerzillaService.AddGame(gameInfo1, user.id, user.admin);

            return Ok(result);
        }

        [BasicAuth]
        [HttpPost("game/image")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AddGameImage([FromForm] GameImageUpload upload)
        {
            if (upload.ImageFile == null || upload.ImageFile.Length == 0)
                return BadRequest("No file uploaded.");

            
            await ValidateClaim();
            var user = _userService.GetCurrentUser();
            if (user == null) return Unauthorized();

            using (Stream s = upload.ImageFile.OpenReadStream())
            {
                
                bool result = await _gamerzillaService.AddGameImage(
                    upload.Game,
                    s,
                    user.id,
                    user.admin
                );

                return result ? Ok() : NotFound();
            }
        }

        [Route("game/image/show")]
        [HttpGet]
        public Task<IActionResult> ShowGameImage1(string game)
        {
            return ShowGameImage(game);
        }

        [HttpPost]
        [Route("game/image/show")]
        public Task<IActionResult> ShowGameImage2([FromForm] string game)
        {
            return ShowGameImage(game);
        }

        private async Task<IActionResult> ShowGameImage(string game)
        {
            var s = await _gamerzillaService.GetGameImage(game);
            if (s != null)
            {
                return new FileStreamResult(s, "image/png");
            }
            else
                return NotFound();
        }

        [BasicAuth]
        [HttpPost("trophy/image")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AddTrophyImage([FromForm] TrophyImageUpload upload)
        {
            if (upload.TrueImageFile == null || upload.TrueImageFile.Length == 0 ||
                upload.FalseImageFile == null || upload.FalseImageFile.Length == 0)
            {
                return BadRequest("Both image files are required.");
            }

            using (Stream s1 = upload.TrueImageFile.OpenReadStream())
            using (Stream s2 = upload.FalseImageFile.OpenReadStream())
            {
                bool result = await _gamerzillaService.AddTrophyImage(upload.Game, upload.Trophy, s1, s2);
                return result ? Ok() : NotFound();
            }
        }

        [Route("trophy/image/show")]
        [HttpGet]
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
            var s = await _gamerzillaService.GetTrophyImage(game, trophy, achieved);
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
            if (await _gamerzillaService.SetUserStat(game, trophy, _sessionContext.UserId, true, 0))
                return Ok();
            else
                return NotFound();
        }

        [BasicAuth]
        [HttpPost]
        [Route("trophy/set/stat")]
        public async Task<IActionResult> SetTrophyStat([FromForm] string game, [FromForm] string trophy, [FromForm] int progress)
        {
            if (await _gamerzillaService.SetUserStat(game, trophy, _sessionContext.UserId, false, progress))
                return Ok();
            else
                return NotFound();
        }


    }
}
