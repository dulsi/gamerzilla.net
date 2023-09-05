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
using backend.Context;
using backend.Models;

namespace backend.Service;

public class GamerzillaService
{
    private readonly GamerzillaContext _context;

    static private readonly string _getGames1Sqlite = "select shortname, gamename, (select count(*) from userstat u2 where u2.achieved = 1 and g.id = u2.gameid and u2.userid = @USERID) as earned, (select count(*) from trophy t where g.id = t.gameid) as total_trophy, (select max(u2.id) from userstat u2 where u2.achieved = 1 and g.id = u2.gameid and u2.userid = @USERID) as sortfield from game g where g.id in (select gameid from userstat u where u.userid = @USERID) order by sortfield desc limit @LIMIT offset @OFFSET";
    static private readonly string _getGamesCountSqlite = "select count(*) from game g where g.id in (select gameid from userstat u where u.userid = @USERID)";
    static private readonly string _getGames2Sqlite = "select shortname, gamename, (select count(*) from userstat u2 where u2.achieved = 1 and g.id = u2.gameid and u2.userid = @USERID) as earned, (select count(*) from trophy t where g.id = t.gameid) as total_trophy, (select max(u2.id) from userstat u2 where u2.achieved = 1 and g.id = u2.gameid and u2.userid = @USERID) as sortfield from game g where g.id in (select gameid from userstat u where u.userid = @USERID) order by sortfield desc";

    static private readonly string _getGames1Postgres = "select \"ShortName\", \"GameName\", (select count(*) from \"UserStat\" u2 where u2.\"Achieved\" = true and g.\"Id\" = u2.\"GameId\" and u2.\"UserId\" = @USERID) as earned, (select count(*) from \"Trophy\" t where g.\"Id\" = t.\"GameId\") as total_trophy, (select max(u2.\"Id\") from \"UserStat\" u2 where u2.\"Achieved\" = true and g.\"Id\" = u2.\"GameId\" and u2.\"UserId\" = @USERID) as sortfield from \"Game\" g where g.\"Id\" in (select \"GameId\" from \"UserStat\" u where u.\"UserId\" = @USERID) order by sortfield desc limit @LIMIT offset @OFFSET";
    static private readonly string _getGamesCountPostgres = "select count(*) from \"Game\" g where g.\"Id\" in (select \"GameId\" from \"UserStat\" u where u.\"UserId\" = @USERID)";
    static private readonly string _getGames2Postgres = "select \"ShortName\", \"GameName\", (select count(*) from \"UserStat\" u2 where u2.\"Achieved\" = true and g.\"Id\" = u2.\"GameId\" and u2.\"UserId\" = @USERID) as earned, (select count(*) from \"Trophy\" t where g.\"Id\" = t.\"GameId\") as total_trophy, (select max(u2.\"Id\") from \"UserStat\" u2 where u2.\"Achieved\" = true and g.\"Id\" = u2.\"GameId\" and u2.\"UserId\" = @USERID) as sortfield from \"Game\" g where g.\"Id\" in (select \"GameId\" from \"UserStat\" u where u.\"UserId\" = @USERID) order by sortfield desc";

    public GamerzillaService(GamerzillaContext context)
    {
        _context = context;
    }

    public GameSummary GetPagedGames(int userId, int pagesize, int currentpage)
    {
        DbConnection connection = _context.Database.GetDbConnection();
        GameSummary result = new GameSummary();
        result.currentPage = currentpage;
        result.pageSize = pagesize;

        try
        {
            connection.Open();

            int totalRead = 0;
            using (DbCommand command = connection.CreateCommand())
            {
                if (_context.Database.IsNpgsql())
                {
                    command.CommandText = _getGames1Postgres;
                    command.Parameters.Add(new NpgsqlParameter("@USERID", userId));
                    command.Parameters.Add(new NpgsqlParameter("@LIMIT", result.pageSize));
                    command.Parameters.Add(new NpgsqlParameter("@OFFSET", result.currentPage * result.pageSize));
                }
                else
                {
                    command.CommandText = _getGames1Sqlite;
                    command.Parameters.Add(new SqliteParameter("@USERID", userId));
                    command.Parameters.Add(new SqliteParameter("@LIMIT", result.pageSize));
                    command.Parameters.Add(new SqliteParameter("@OFFSET", result.currentPage * result.pageSize));
                }

                using (DbDataReader dataReader = command.ExecuteReader())
                    if (dataReader.HasRows)
                        while (dataReader.Read())
                        {
                            GameShort summary = new GameShort();
                            summary.shortname = dataReader.GetString(0);
                            summary.name = dataReader.GetString(1);
                            summary.earned = dataReader.GetString(2);
                            summary.total = dataReader.GetString(3);
                            result.games.Add(summary);
                            totalRead++;
                        }
            }
            if (totalRead != result.pageSize)
            {
                result.totalPages = result.currentPage + 1;
            }
            else
            {
                using (DbCommand command = connection.CreateCommand())
                {
                    if (_context.Database.IsNpgsql())
                    {
                        command.CommandText = _getGamesCountPostgres;
                        command.Parameters.Add(new NpgsqlParameter("@USERID", userId));
                    }
                    else
                    {
                        command.CommandText = _getGamesCountSqlite;
                        command.Parameters.Add(new SqliteParameter("@USERID", userId));
                    }

                    using (DbDataReader dataReader = command.ExecuteReader())
                        if (dataReader.HasRows)
                            while (dataReader.Read())
                            {
                                int total = dataReader.GetInt32(0);
                                result.totalPages = total / result.pageSize + (total % result.pageSize > 0 ? 1 : 0);
                            }
                }
            }
        }

        catch (System.Exception) { }

        finally { connection.Close(); }

        return result;
    }

    public IList<GameShort> GetGames(int userId)
    {
        DbConnection connection = _context.Database.GetDbConnection();
        IList<GameShort> result = new List<GameShort>();

        try
        {
            connection.Open();

            using (DbCommand command = connection.CreateCommand())
            {
                if (_context.Database.IsNpgsql())
                {
                    command.CommandText = _getGames2Postgres;
                    command.Parameters.Add(new NpgsqlParameter("@USERID", userId));
                }
                else
                {
                    command.CommandText = _getGames2Sqlite;
                    command.Parameters.Add(new SqliteParameter("@USERID", userId));
                }

                using (DbDataReader dataReader = command.ExecuteReader())
                    if (dataReader.HasRows)
                        while (dataReader.Read())
                        {
                            GameShort summary = new GameShort();
                            summary.shortname = dataReader.GetString(0);
                            summary.name = dataReader.GetString(1);
                            summary.earned = dataReader.GetString(2);
                            summary.total = dataReader.GetString(3);
                            result.Add(summary);
                        }
            }
        }

        catch (System.Exception) { }

        finally { connection.Close(); }

        return result;
    }

    public async Task<GameApi1> AddGame(GameApi1 gameInfo1, int userId)
    {
        Game gameInfo = await _context.Games.FirstOrDefaultAsync(g => g.ShortName == gameInfo1.shortname);
        if (gameInfo != null)
        {
            gameInfo.Trophies = await _context.Trophies.Include(t => t.Stat
                .Where(s => s.UserId == userId && s.GameId == gameInfo.Id) ).Where(t => t.GameId == gameInfo.Id).ToListAsync();
            return gameInfo1;
        }
        else
        {
            gameInfo = new Game();
            gameInfo.Import(gameInfo1);
            _context.Games.Add(gameInfo);
            await _context.SaveChangesAsync();
            foreach (var t in gameInfo1.trophy)
            {
                int trophyId = 0;
                foreach (var t2 in gameInfo.Trophies)
                {
                    if (t2.TrophyName == t.trophy_name)
                    {
                        trophyId = t2.Id;
                        break;
                    }
                }
                if (t.achieved == "1")
                    await SetUserStat(gameInfo.Id, trophyId, userId, true, 0);
                else if (t.progress != "0")
                    await SetUserStat(gameInfo.Id, trophyId, userId, false, Int32.Parse(t.progress));
            }
            return gameInfo1;
        }
    }

    public GameApi1 GetGame(string game, int userId)
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

    public async Task<bool> AddGameImage(string game, Stream imgFile)
    {
        Game gameInfo = await _context.Games.FirstOrDefaultAsync(g => g.ShortName == game);
        if (gameInfo != null)
        {
            backend.Models.Image img = new backend.Models.Image();
            img.GameId = gameInfo.Id;
            img.TrophyId = -1;
            img.Achieved = true;
            img.data = ResizeImage(imgFile, 368, 172);
            _context.Images.Add(img);
            await _context.SaveChangesAsync();
            return true;
        }
        else
            return false;
    }

    public async Task<Stream> GetGameImage(string game)
    {
        Game gameInfo = await _context.Games.FirstOrDefaultAsync(g => g.ShortName == game);
        if (gameInfo != null)
        {
            backend.Models.Image img = await _context.Images.FirstOrDefaultAsync(i => i.GameId == gameInfo.Id && i.TrophyId == -1);
            return new MemoryStream(img.data);
        }
        else
            return null;
    }

    public async Task<bool> AddTrophyImage(string game, string trophy, Stream trueFile, Stream falseFile)
    {
        Game gameInfo = await _context.Games.FirstOrDefaultAsync(g => g.ShortName == game);
        Trophy trophyInfo = await _context.Trophies.FirstOrDefaultAsync(t => t.TrophyName == trophy && t.GameId == gameInfo.Id);
        if (gameInfo != null)
        {
            backend.Models.Image trueImg = new backend.Models.Image();
            trueImg.GameId = gameInfo.Id;
            trueImg.TrophyId = trophyInfo.Id;
            trueImg.Achieved = true;
            trueImg.data = ResizeImage(trueFile, 64, 64);
            _context.Images.Add(trueImg);
            backend.Models.Image falseImg = new backend.Models.Image();
            falseImg.GameId = gameInfo.Id;
            falseImg.TrophyId = trophyInfo.Id;
            falseImg.Achieved = false;
            falseImg.data = ResizeImage(falseFile, 64, 64);
            _context.Images.Add(falseImg);
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
        SixLabors.ImageSharp.Image imgOrig = SixLabors.ImageSharp.Image.Load(memStream, new SixLabors.ImageSharp.Formats.Png.PngDecoder());
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
