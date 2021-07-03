using System.Linq;
using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.Context;

namespace backend.Service
{
    public class UserService
    {
        SessionContext _sessionContext;
        UserContext _context;

        public UserService(SessionContext sessionContext, UserContext userContext)
        {
            _sessionContext = sessionContext;
            _context = userContext;
            _context.Database.EnsureCreated();
        }

        public bool IsValidUser(string userName, string password)
        {
            User user = _context.Users.FirstOrDefault(g => g.UserName == userName);
            if ((user != null) && (password == user.Password))
            {
                _sessionContext.UserName = userName;
                _sessionContext.UserId = user.Id;
                return true;
            }
            else
                return false;
        }

        public int findUser(string userName)
        {
            User user = _context.Users.FirstOrDefault(g => g.UserName == userName);
            if (user != null)
                return user.Id;
            else
                return 0;
        }
    }
}
