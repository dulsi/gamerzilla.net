using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
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
        private readonly GamerzillaContext _context;
        private readonly SessionContext _sessionContext;
        private readonly UserService _userService;

        public GamerzillaController(ILogger<GamerzillaController> logger, GamerzillaContext context, SessionContext sessionContext, UserService userService)
        {
            _logger = logger;
            _context = context;
            _context.Database.EnsureCreated();
            _sessionContext = sessionContext;
            _userService = userService;
        }

        [Route("games")]
        public IList<GameSummary> GetGames1(string username)
        {
            int userId = 0;
            if (username != null)
            {
                userId = _userService.findUser(username);
            }
            else
                userId = _sessionContext.UserId;
            return GetGames(userId);
        }

        [BasicAuth]
        [HttpPost]
        [Route("games")]
        public IList<GameSummary> GetGames2()
        {
            return GetGames(_sessionContext.UserId);
        }

        public IList<GameSummary> GetGames(int userId)
        {
            DbConnection connection = _context.Database.GetDbConnection();
            IList<GameSummary> result = new List<GameSummary>();

            try
            {
                connection.Open();

                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "select shortname, gamename, (select count(*) from userstat u2 where u2.achieved = @USERID and g.id = u2.gameid and u2.userid = 1) as earned, (select count(*) from trophy t where g.id = t.gameid) as total_trophy from game g where g.id in (select gameid from userstat u where u.userid = @USERID)";
                    command.Parameters.Add(new SqliteParameter("@USERID", userId));

                    using (DbDataReader dataReader = command.ExecuteReader())
                        if (dataReader.HasRows)
                            while (dataReader.Read())
                            {
                                GameSummary summary = new GameSummary();
                                summary.shortname = dataReader.GetString(0);
                                summary.name = dataReader.GetString(1);
                                summary.earned = dataReader.GetInt32(2);
                                summary.total = dataReader.GetInt32(3);
                                result.Add(summary);
                            }
                }
            }

            catch (System.Exception) { }

            finally { connection.Close(); }

            return result;
        }

        [Route("game")]
        public GameApi1 GetGame1(string game, string username)
        {
            int userId = 0;
            if (username != null)
            {
                userId = _userService.findUser(username);
            }
            else
                userId = _sessionContext.UserId;
            return GetGame(game, userId);
        }

        [BasicAuth]
        [HttpPost]
        [Route("game")]
        public GameApi1 GetGame2([FromForm] string game)
        {
            return GetGame(game, _sessionContext.UserId);
        }

        private GameApi1 GetGame(string game, int userId)
        {
            Game gameInfo = _context.Games.FirstOrDefault(g => g.ShortName == game);
            GameApi1 gameInfo1 = null;
            if (gameInfo != null)
            {
                gameInfo1 = new GameApi1();
                gameInfo.Trophies = _context.Trophies.Include(t => t.Stat .Where(s => s.UserId == userId && s.GameId == gameInfo.Id) ).Where(t => t.GameId == gameInfo.Id).ToList();
                gameInfo.Export(gameInfo1);
            }
            return gameInfo1;
        }

        [BasicAuth]
        [HttpPost]
        [Route("game/add")]
        public GameApi1 AddGame([FromForm] string game)
        {
            _logger.LogInformation("AddGame");
            Game gameInfo = _context.Games.FirstOrDefault(g => g.ShortName == game);
            GameApi1 gameInfo1 = JsonConvert.DeserializeObject<GameApi1>(game);
            if (gameInfo != null)
            {
                gameInfo.Trophies = _context.Trophies.Include(t => t.Stat
                    .Where(s => s.UserId == _sessionContext.UserId && s.GameId == gameInfo.Id) ).Where(t => t.GameId == gameInfo.Id).ToList();
                return gameInfo1;
            }
            else
            {
                gameInfo = new Game();
                gameInfo.Import(gameInfo1);
                _context.Games.Add(gameInfo);
                _context.SaveChanges();
                return gameInfo1;
            }
        }

        [BasicAuth]
        [HttpPost]
        [Route("game/image")]
        public async Task<IActionResult> AddGameImage([FromForm] string game, [FromForm] IFormFile imagefile)
        {
            Game gameInfo = _context.Games.FirstOrDefault(g => g.ShortName == game);
            if (gameInfo != null)
            {
                using (Stream s = imagefile.OpenReadStream())
                {
                    backend.Models.Image img = new backend.Models.Image();
                    img.GameId = gameInfo.Id;
                    img.TrophyId = -1;
                    img.Achieved = true;
                    img.data = ResizeImage(s, 368, 172);
                    _context.Images.Add(img);
                    _context.SaveChanges();
                }
                return Ok();
            }
            else
                return NotFound();
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
            Game gameInfo = _context.Games.FirstOrDefault(g => g.ShortName == game);
            if (gameInfo != null)
            {
                backend.Models.Image img = _context.Images.FirstOrDefault(i => i.GameId == gameInfo.Id && i.TrophyId == -1);
                Stream s = new MemoryStream(img.data);
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
            Game gameInfo = _context.Games.FirstOrDefault(g => g.ShortName == game);
            Trophy trophyInfo = _context.Trophies.FirstOrDefault(t => t.TrophyName == trophy && t.GameId == gameInfo.Id);
            if (gameInfo != null)
            {
                using (Stream s1 = trueimagefile.OpenReadStream())
                {
                    backend.Models.Image img = new backend.Models.Image();
                    img.GameId = gameInfo.Id;
                    img.TrophyId = trophyInfo.Id;
                    img.Achieved = true;
                    img.data = ResizeImage(s1, 64, 64);
                    _context.Images.Add(img);
                }
                using (Stream s2 = falseimagefile.OpenReadStream())
                {
                    backend.Models.Image img = new backend.Models.Image();
                    img.GameId = gameInfo.Id;
                    img.TrophyId = trophyInfo.Id;
                    img.Achieved = false;
                    img.data = ResizeImage(s2, 64, 64);
                    _context.Images.Add(img);
                }
                _context.SaveChanges();
                return Ok();
            }
            else
                return NotFound();
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
            Game gameInfo = _context.Games.FirstOrDefault(g => g.ShortName == game);
            Trophy trophyInfo = _context.Trophies.FirstOrDefault(t => t.TrophyName == trophy && t.GameId == gameInfo.Id);
            if (gameInfo != null)
            {
                backend.Models.Image img = _context.Images.FirstOrDefault(i => i.GameId == gameInfo.Id && i.TrophyId == trophyInfo.Id && (i.Achieved == (achieved == 1)));
                Stream s = new MemoryStream(img.data);
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
            Game gameInfo = _context.Games.FirstOrDefault(g => g.ShortName == game);
            if (gameInfo == null)
                return NotFound();
            Trophy trophyInfo = _context.Trophies.FirstOrDefault(t => t.TrophyName == trophy && t.GameId == gameInfo.Id);
            if (trophyInfo == null)
                return NotFound();
            UserStat userStat = _context.UserStats.FirstOrDefault(u => u.UserId == _sessionContext.UserId && u.TrophyId == trophyInfo.Id && u.GameId == gameInfo.Id);
            if (userStat != null)
            {
                userStat.Achieved = true;
                _context.SaveChanges();
                return Ok();
            }
            else
            {
                userStat = new UserStat();
                userStat.GameId = gameInfo.Id;
                userStat.TrophyId = trophyInfo.Id;
                userStat.UserId = 1;
                userStat.Achieved = true;
                userStat.Progress = 0;
                _context.UserStats.Add(userStat);
                _context.SaveChanges();
                return Ok();
            }
        }

        [BasicAuth]
        [HttpPost]
        [Route("trophy/set/stat")]
        public async Task<IActionResult> AddTrophyImage([FromForm] string game, [FromForm] string trophy, [FromForm] int progress)
        {
            Game gameInfo = _context.Games.FirstOrDefault(g => g.ShortName == game);
            if (gameInfo == null)
                return NotFound();
            Trophy trophyInfo = _context.Trophies.FirstOrDefault(t => t.TrophyName == trophy && t.GameId == gameInfo.Id);
            if (trophyInfo == null)
                return NotFound();
            UserStat userStat = _context.UserStats.FirstOrDefault(u => u.UserId == _sessionContext.UserId && u.TrophyId == trophyInfo.Id && u.GameId == gameInfo.Id);
            if (userStat != null)
            {
                if (progress > userStat.Progress)
                {
                    userStat.Progress = progress;
                    _context.SaveChanges();
                }
                return Ok();
            }
            else
            {
                userStat = new UserStat();
                userStat.GameId = gameInfo.Id;
                userStat.TrophyId = trophyInfo.Id;
                userStat.UserId = 1;
                userStat.Achieved = false;
                userStat.Progress = progress;
                _context.UserStats.Add(userStat);
                _context.SaveChanges();
                return Ok();
            }
        }
        
        private byte[] ResizeImage(Stream s, int w, int h)
        {
            SixLabors.ImageSharp.Image imgOrig = SixLabors.ImageSharp.Image.Load(s, new SixLabors.ImageSharp.Formats.Png.PngDecoder());
            SixLabors.ImageSharp.Image imgNew = imgOrig.Clone(x => x.Resize(new ResizeOptions
            {
                Size = new Size(w, h),
                Mode = ResizeMode.Stretch
            }));
            MemoryStream memStream = new MemoryStream(20000);
            imgNew.Save(memStream, new SixLabors.ImageSharp.Formats.Png.PngEncoder());
            memStream.Seek(0, SeekOrigin.Begin);
            byte[] data = new byte[memStream.Length];
            memStream.Read(data, 0, (int)memStream.Length);
            return data;
        }
    }
}
