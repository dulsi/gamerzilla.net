using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using backend.Models;
using backend.Helpers;
using Microsoft.Extensions.Configuration;

namespace backend.Service;

public class GamerzillaService
{
    private readonly GamerzillaContext _context;
    private readonly GamerzillaContext _userContext;
    private readonly IConfiguration _config;
    private readonly EmailService _emailService;


    public GamerzillaService(GamerzillaContext context, GamerzillaContext userContext, IConfiguration config, EmailService emailService)
    {
        _context = context;
        _userContext = userContext;
        _config = config;
        _emailService = emailService;
    }


    public async Task<string> TransferGameAsync(int gameId, int newOwnerId)
    {
        
        string mode = _config["AppMode"] ?? "Standalone";
        bool isHubzilla = mode.Equals("Hubzilla", StringComparison.OrdinalIgnoreCase);
        bool emailEnabled = _config.GetValue<bool>("EmailSettings:Enabled");

        
        if (isHubzilla || !emailEnabled)
        {
            
            var game = await _context.Games.FindAsync(gameId);
            if (game == null) throw new Exception("Game not found");

            game.OwnerId = newOwnerId;
            await _context.SaveChangesAsync();

            return "Transfer complete.";
        }
        else
        {
            
            
            

            return "Verification email sent.";
        }
    }



    public async Task<object> GetOwnedGamesWithStatsAsync(int userId, int pagesize, int currentpage)
    {
        int offset = currentpage * pagesize;

        
        var query = _context.Games.Where(g => g.OwnerId == userId);

        
        int totalRecords = await query.CountAsync();
        int totalPages = (totalRecords + pagesize - 1) / pagesize;

        
        var data = await query
            .OrderBy(g => g.GameName)
            .Skip(offset)
            .Take(pagesize)
            .Select(g => new
            {
                g.Id,
                g.ShortName,
                g.GameName,
                g.OwnerId,
                
                Earned = _context.UserStats.Count(u => u.Achieved && u.GameId == g.Id && u.UserId == userId),
                Total = _context.Trophies.Count(t => t.GameId == g.Id)
            })
            .ToListAsync();

        
        var games = data.Select(item => new
        {
            id = item.Id,
            shortname = item.ShortName,
            name = item.GameName,
            ownerId = item.OwnerId,
            
            earned = item.Earned.ToString(),
            total = item.Total.ToString()
        }).ToList();

        return new
        {
            games = games, 
            totalRecords,
            totalPages,
            currentPage = currentpage,
            pageSize = pagesize
        };
    }

    public async Task<object> GetOwnedGamesAsync(int userId, bool isAdmin, int pagesize, int currentpage)
    {
        
        int offset = currentpage * pagesize;

        
        IQueryable<Game> query = _context.Games;

        if (!isAdmin)
        {
            
            query = query.Where(g => g.OwnerId == userId);
        }

        
        query = query.OrderBy(g => g.GameName);

        
        int totalRecords = await query.CountAsync();
        
        int totalPages = (totalRecords + pagesize - 1) / pagesize;

        
        
        var data = await query
            .Skip(offset)
            .Take(pagesize)
            .Select(g => new
            {
                g.Id,
                g.GameName,
                g.ShortName,
                OwnerId = g.OwnerId
            })
            .ToListAsync();

        
        var ownerIds = data
            .Where(g => g.OwnerId.HasValue)
            .Select(g => g.OwnerId.Value)
            .Distinct()
            .ToList();

        
        var ownerNames = await _userContext.Users
            .Where(u => ownerIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.UserName);

        
        var games = data.Select(item => new
        {
            id = item.Id,
            gameName = item.GameName,
            shortName = item.ShortName,
            
            ownerName = (item.OwnerId.HasValue)
                ? (ownerNames.ContainsKey(item.OwnerId.Value) ? ownerNames[item.OwnerId.Value] : "Unknown")
                : "Unclaimed"
        }).ToList();

        
        return new
        {
            data = games,
            total = totalRecords,
            totalPages = totalPages,
            currentPage = currentpage
        };
    }

    
    private string CreateTransferToken(int gameId, int newOwnerId)
    {
        
        string secret = Guid.NewGuid().ToString("N");

        
        
        string payload = $"{gameId}:{newOwnerId}:{secret}";

        
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(payload);
        return Convert.ToBase64String(plainTextBytes);
    }



    //
    //public async Task<string> TransferOwnershipAsync(int requestorId, bool isAdmin, int gameId, int newOwnerId)
    //{
    
    

    
    

    
    
    

    
    
    
    
    
    
    
    
    
    
    
    
    

    
    

    
    
    
    

    
    
    
    

    

    
    
    //}

    //
    //public async Task<string> VerifyGameTransferAsync(string token)
    //{
    
    
    

    

    
    
    

    
    
    
    

    
    
    
    
    
    

    
    

    
    

    
    
    //}


    public async Task<string> TransferOwnershipAsync(int requestorId, bool isAdmin, int gameId, int newOwnerId)
    {
        var game = await _context.Games.FindAsync(gameId);
        if (game == null) return "Game not found";

        if (game.OwnerId == newOwnerId)
        {
            return "Game is already owned by that user.";
        }

        
        if (game.OwnerId != requestorId && !isAdmin) return "Unauthorized";

        
        var recipient = await _context.Users.FindAsync(newOwnerId);
        if (recipient == null) return "Recipient user not found";

        string mode = _config["AppMode"] ?? "Standalone";
        bool isHubzilla = mode.Equals("Hubzilla", StringComparison.OrdinalIgnoreCase);
        bool emailEnabled = _config.GetValue<bool>("EmailSettings:Enabled");

        
        if (isHubzilla || !emailEnabled || (game.OwnerId == null && isAdmin))
        {
            game.OwnerId = newOwnerId;
            await _context.SaveChangesAsync();
            return "Success";
        }
        
        else
        {
            
            string token = CreateTransferToken(gameId, newOwnerId);

            
            
            recipient.VerificationToken = token;
            await _context.SaveChangesAsync();

            
            
            var sender = await _context.Users.FindAsync(game.OwnerId);
            string senderName = sender?.UserName ?? "An administrator";

            await _emailService.SendTransferVerificationEmailAsync(
                recipient.Email,       
                recipient.UserName,    
                senderName,            
                game.GameName,
                token
            );

            return "Verification email sent to " + recipient.UserName;
        }
    }

    public async Task<string> VerifyGameTransferAsync(string token)
    {
        var data = TokenHelper.DecodeTransferToken(token);
        if (data == null) return "Invalid token format.";

        var (gameId, newOwnerId, secret) = data.Value;

        
        var recipient = await _context.Users.FindAsync(newOwnerId);
        if (recipient == null) return "Recipient not found.";

        
        if (recipient.VerificationToken != token)
        {
            return "Transfer link is expired or already used.";
        }

        
        var game = await _context.Games.FindAsync(gameId);
        if (game == null) return "Game no longer exists.";

        
        game.OwnerId = newOwnerId;

        
        recipient.VerificationToken = null;

        await _context.SaveChangesAsync();
        return "Success";
    }

    public GameSummary GetPagedGames(int userId, int pagesize, int currentpage)
    {
        GameSummary result = new GameSummary();
        result.currentPage = currentpage;
        result.pageSize = pagesize;

        int offset = currentpage * pagesize;

        
        var query = _context.Games
            .Where(g =>
                
                _context.UserStats.Any(u => u.UserId == userId && u.GameId == g.Id)
                ||
                
                g.OwnerId == userId
            )
            .Select(g => new
            {
                g.Id, 
                g.ShortName,
                g.GameName,
                g.OwnerId, 

                
                Earned = _context.UserStats.Count(u => u.Achieved && u.GameId == g.Id && u.UserId == userId),
                Total = _context.Trophies.Count(t => t.GameId == g.Id),

                
                SortField = _context.UserStats
                    .Where(u => u.Achieved && u.GameId == g.Id && u.UserId == userId)
                    .Max(u => (int?)u.Id) ?? 0
            });

        int totalRecords = query.Count();
        result.totalPages = (totalRecords + pagesize - 1) / pagesize;

        var data = query
            .OrderByDescending(x => x.SortField)
            //.ThenBy(x => x.GameName)
            .Skip(offset)
            .Take(pagesize)
            .ToList();

        foreach (var item in data)
        {

            var game = new GameShort
            {
                id = item.Id,
                shortname = item.ShortName,
                name = item.GameName,
                earned = item.Earned.ToString(),
                total = item.Total.ToString(),
                ownerId = item.OwnerId
            };
            result.games.Add(game);
        }

        return result;
    }


    public IList<GameShort> GetGames(int userId)
    {
        var query = _context.Games
            .Where(g => _context.UserStats.Any(u => u.UserId == userId && u.GameId == g.Id))
            .Select(g => new
            {
                g.Id,
                g.OwnerId,
                g.ShortName,
                g.GameName,
                
                Earned = _context.UserStats.Count(u => u.Achieved && u.GameId == g.Id && u.UserId == userId),
                
                Total = _context.Trophies.Count(t => t.GameId == g.Id),
                
                SortField = _context.UserStats
                    .Where(u => u.Achieved && u.GameId == g.Id && u.UserId == userId)
                    .Max(u => (int?)u.Id) ?? 0
            });

        
        
        var data = query
            .OrderByDescending(x => x.SortField)
            .ToList();

        
        
        var result = data.Select(item => new GameShort
        {
            id = item.Id,
            ownerId = item.OwnerId,
            shortname = item.ShortName,
            name = item.GameName,
            earned = item.Earned.ToString(),
            total = item.Total.ToString()
        }).ToList();

        return result;
    }


    public async Task<GameApi1> AddGame(GameApi1 gameInfo1, int userId, bool isAdmin)
    {
        
        Game game = await _context.Games
            .Include(g => g.Trophies)
            .ThenInclude(t => t.Stat.Where(s => s.UserId == userId))
            .AsSplitQuery()
            .FirstOrDefaultAsync(g => g.ShortName == gameInfo1.shortname);

        int authority = 1; 

        
        if (game != null)
        {
            bool isOwner = (game.OwnerId == userId) || isAdmin;
            int newVersion = 0;
            int.TryParse(gameInfo1.version, out newVersion);

            if (isOwner && newVersion > game.VersionNum)
            {
                game.VersionNum = newVersion;
                game.GameName = gameInfo1.name;
                authority = 2; 
            }
            else if (newVersion < game.VersionNum)
            {
                gameInfo1.version = game.VersionNum.ToString();
            }
            else if (!isOwner && newVersion > game.VersionNum)
            {
                gameInfo1.version = game.VersionNum.ToString();
            }
        }
        else
        {
            
            game = new Game();
            game.Import(gameInfo1);
            game.OwnerId = userId;
            _context.Games.Add(game);
            await _context.SaveChangesAsync();

            
            
            authority = 2;
        }

        
        HashSet<string> usedTrophies = new HashSet<string>();

        
        foreach (var dbTrophy in game.Trophies)
        {
            var inputTrophy = gameInfo1.trophy.FirstOrDefault(t => t.trophy_name == dbTrophy.TrophyName);

            if (inputTrophy != null)
            {
                usedTrophies.Add(dbTrophy.TrophyName);

                
                if (dbTrophy.TrophyDescription != inputTrophy.trophy_desc)
                {
                    if (authority == 1)
                    {
                        
                        inputTrophy.trophy_desc = dbTrophy.TrophyDescription;
                        inputTrophy.max_progress = dbTrophy.MaxProgress.ToString();
                    }
                    else
                    {
                        
                        dbTrophy.TrophyDescription = inputTrophy.trophy_desc;
                        int mp;
                        if (int.TryParse(inputTrophy.max_progress, out mp)) dbTrophy.MaxProgress = mp;
                    }
                }

                
                if (inputTrophy.achieved != "0" || inputTrophy.progress != "0")
                {
                    var stat = dbTrophy.Stat.FirstOrDefault(s => s.UserId == userId);
                    if (stat == null)
                    {
                        stat = new UserStat { UserId = userId, GameId = game.Id, TrophyId = dbTrophy.Id };
                        dbTrophy.Stat.Add(stat);
                        _context.UserStats.Add(stat);
                    }

                    if (inputTrophy.achieved == "1") stat.Achieved = true;

                    int prog = 0;
                    int.TryParse(inputTrophy.progress, out prog);
                    stat.Progress = prog;
                }
            }
            else
            {
                
                var stat = dbTrophy.Stat.FirstOrDefault(s => s.UserId == userId);
                gameInfo1.trophy.Add(new TrophyApi1
                {
                    trophy_name = dbTrophy.TrophyName,
                    trophy_desc = dbTrophy.TrophyDescription,
                    max_progress = dbTrophy.MaxProgress.ToString(),
                    achieved = (stat != null && stat.Achieved) ? "1" : "0",
                    progress = (stat != null ? stat.Progress : 0).ToString()
                });
            }
        }

        
        var newTrophies = gameInfo1.trophy
            .Where(t => !usedTrophies.Contains(t.trophy_name)
                     && !game.Trophies.Any(dbT => dbT.TrophyName == t.trophy_name))
            .ToList();

        
        if (authority == 2)
        {
            foreach (var t in newTrophies)
            {
                int maxProg = 0;
                int.TryParse(t.max_progress, out maxProg); 

                var newDbTrophy = new Trophy
                {
                    GameId = game.Id,
                    TrophyName = t.trophy_name,
                    TrophyDescription = t.trophy_desc,
                    MaxProgress = maxProg
                };
                _context.Trophies.Add(newDbTrophy);

                
                if (t.achieved != "0" || t.progress != "0")
                {
                    int prog = 0;
                    int.TryParse(t.progress, out prog);

                    var stat = new UserStat
                    {
                        UserId = userId,
                        GameId = game.Id,
                        trophy = newDbTrophy, 
                        Achieved = (t.achieved == "1"),
                        Progress = prog
                    };
                    _context.UserStats.Add(stat);
                }
            }
        }

        
        await _context.SaveChangesAsync();

        return gameInfo1;
    }


    public GameApi1 GetGame(string game, int userId)
    {
        
        Game gameInfo = _context.Games
            .Include(g => g.Trophies)
                .ThenInclude(t => t.Stat.Where(s => s.UserId == userId))
            .AsSplitQuery() 
            .FirstOrDefault(g => g.ShortName == game);

        if (gameInfo == null) return null;

        GameApi1 gameInfo1 = new GameApi1();
        gameInfo.Export(gameInfo1);
        return gameInfo1;
    }

    //public GameApi1 GetGame(string game, int userId)
    //{
    
    
    
    
    
    
    
    
    
    //}

    public async Task<bool> AddGameImage(string gameShortName, Stream imgFile, int userId, bool isAdmin)
    {
        
        Game gameInfo = await _context.Games.FirstOrDefaultAsync(g => g.ShortName == gameShortName);

        
        if (gameInfo == null) return false;

        
        
        if (gameInfo.OwnerId != null && gameInfo.OwnerId != userId && !isAdmin)
        {
            
            
            
            return false;

            
            
        }

        

        
        var existingImg = await _context.Images
            .FirstOrDefaultAsync(i => i.GameId == gameInfo.Id && i.TrophyId == -1);

        
        byte[] newData = ResizeImage(imgFile, 368, 172);

        if (existingImg != null)
        {
            
            existingImg.data = newData;
            existingImg.Achieved = true;
        }
        else
        {
            
            backend.Models.Image img = new backend.Models.Image();
            img.GameId = gameInfo.Id;
            img.TrophyId = -1;
            img.Achieved = true;
            img.data = newData;
            _context.Images.Add(img);
        }

        await _context.SaveChangesAsync();
        return true;
    }



    public async Task<Stream> GetGameImage(string game)
    {
        Game gameInfo = await _context.Games.FirstOrDefaultAsync(g => g.ShortName == game);
        if (gameInfo != null)
        {
            backend.Models.Image img = await _context.Images.FirstOrDefaultAsync(i => i.GameId == gameInfo.Id && i.TrophyId == -1);
            if (img != null)
            {
                return new MemoryStream(img.data);
            } else
            {
                return null;
            }
        }
        else
            return null;
    }

    public async Task<bool> AddTrophyImage(string game, string trophy, Stream trueFile, Stream falseFile)
    {
        Game gameInfo = await _context.Games.FirstOrDefaultAsync(g => g.ShortName == game);

        
        if (gameInfo == null) return false;

        Trophy trophyInfo = await _context.Trophies
            .FirstOrDefaultAsync(t => t.TrophyName == trophy && t.GameId == gameInfo.Id);

        if (trophyInfo != null)
        {
            
            var trueImg = await _context.Images
                .FirstOrDefaultAsync(i => i.GameId == gameInfo.Id && i.TrophyId == trophyInfo.Id && i.Achieved == true);

            byte[] trueData = ResizeImage(trueFile, 64, 64);

            if (trueImg != null)
            {
                trueImg.data = trueData;
            }
            else
            {
                trueImg = new backend.Models.Image();
                trueImg.GameId = gameInfo.Id;
                trueImg.TrophyId = trophyInfo.Id;
                trueImg.Achieved = true;
                trueImg.data = trueData;
                _context.Images.Add(trueImg);
            }

            
            var falseImg = await _context.Images
                .FirstOrDefaultAsync(i => i.GameId == gameInfo.Id && i.TrophyId == trophyInfo.Id && i.Achieved == false);

            byte[] falseData = ResizeImage(falseFile, 64, 64);

            if (falseImg != null)
            {
                falseImg.data = falseData;
            }
            else
            {
                falseImg = new backend.Models.Image();
                falseImg.GameId = gameInfo.Id;
                falseImg.TrophyId = trophyInfo.Id;
                falseImg.Achieved = false;
                falseImg.data = falseData;
                _context.Images.Add(falseImg);
            }

            await _context.SaveChangesAsync();
            return true;
        }
        else
            return false;
    }

    public async Task<Stream> GetTrophyImage(string game, string trophy, int achieved)
    {
        Game gameInfo = await _context.Games.FirstOrDefaultAsync(g => g.ShortName == game);
        Trophy trophyInfo = await _context.Trophies.FirstOrDefaultAsync(t => t.TrophyName == trophy && t.GameId == gameInfo.Id);
        if (gameInfo != null)
        {
            backend.Models.Image img = await _context.Images.FirstOrDefaultAsync(i => i.GameId == gameInfo.Id && i.TrophyId == trophyInfo.Id && (i.Achieved == (achieved == 1)));
            return new MemoryStream(img.data);
        }
        else
            return null;
    }

    public async Task<bool> SetUserStat(string game, string trophy, int userId, bool achieved, int progress)
    {
        Game gameInfo = await _context.Games.FirstOrDefaultAsync(g => g.ShortName == game);
        if (gameInfo == null)
            return false;
        Trophy trophyInfo = await _context.Trophies.FirstOrDefaultAsync(t => t.TrophyName == trophy && t.GameId == gameInfo.Id);
        if (trophyInfo == null)
            return false;
        await SetUserStat(gameInfo.Id, trophyInfo.Id, userId, achieved, progress);
        return true;
    }

    public async Task SetUserStat(int gameId, int trophyId, int userId, bool achieved, int progress)
    {
        UserStat userStat = await _context.UserStats.FirstOrDefaultAsync(u => u.UserId == userId && u.TrophyId == trophyId && u.GameId == gameId);
        if (userStat != null)
        {
            userStat.Achieved = achieved;
            userStat.Progress = progress;
        }
        else
        {
            userStat = new UserStat();
            userStat.GameId = gameId;
            userStat.TrophyId = trophyId;
            userStat.UserId = userId;
            userStat.Achieved = achieved;
            userStat.Progress = progress;
            _context.UserStats.Add(userStat);
        }
        await _context.SaveChangesAsync();
    }

    private byte[] ResizeImage(Stream s, int w, int h)
    {
        MemoryStream memStream = new MemoryStream(20000);
        s.CopyTo(memStream);
        memStream.Seek(0, SeekOrigin.Begin);
        SixLabors.ImageSharp.Image imgOrig = SixLabors.ImageSharp.Image.Load(memStream);
        memStream.Seek(0, SeekOrigin.Begin);
        if (imgOrig.Height != h && imgOrig.Width != w)
        {
            SixLabors.ImageSharp.Image imgNew = imgOrig.Clone(x => x.Resize(new ResizeOptions
            {
                Size = new Size(w, h),
                Mode = ResizeMode.Stretch
            }));
            imgNew.Save(memStream, new SixLabors.ImageSharp.Formats.Png.PngEncoder());
            memStream.Seek(0, SeekOrigin.Begin);
        }
        return memStream.ToArray();
    }
}
