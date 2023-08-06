using System.Linq;
using backend.Context;
using backend.Models;

namespace backend.Service;

public class GamerzillaService
{
    private readonly GamerzillaContext _context;

    public GamerzillaService(GamerzillaContext context)
    {
        _context = context;
    }

    public bool SetUserStat(string game, string trophy, int userId, bool achieved, int progress)
    {
        Game gameInfo = _context.Games.FirstOrDefault(g => g.ShortName == game);
        if (gameInfo == null)
            return false;
        Trophy trophyInfo = _context.Trophies.FirstOrDefault(t => t.TrophyName == trophy && t.GameId == gameInfo.Id);
        if (trophyInfo == null)
            return false;
        SetUserStat(gameInfo.Id, trophyInfo.Id, userId, achieved, progress);
        return true;
    }

    public void SetUserStat(int gameId, int trophyId, int userId, bool achieved, int progress)
    {
        UserStat userStat = _context.UserStats.FirstOrDefault(u => u.UserId == userId && u.TrophyId == trophyId && u.GameId == gameId);
        if (userStat != null)
        {
            userStat.Achieved = achieved;
            userStat.Progress = progress;
            _context.SaveChanges();
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
            _context.SaveChanges();
        }
    }
}
