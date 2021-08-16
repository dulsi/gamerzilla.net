using System.Linq;
using System.Security.Claims;
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
            UserInfo user = _context.Users.FirstOrDefault(g => g.UserName == userName);
            if ((user != null) && (password == user.Password))
            {
                _sessionContext.UserName = userName;
                _sessionContext.UserId = user.Id;
                return true;
            }
            else
                return false;
        }

        public int FindUser(string userName)
        {
            bool admin = false;
            if (_sessionContext.UserId != 0)
            {
                UserInfo current = _context.Users.Find(_sessionContext.UserId);
                if (current != null && current.Admin)
                    admin = current.Admin;
            }
            UserInfo user = _context.Users.FirstOrDefault(g => g.UserName == userName);
            if (user != null)
            {
                if (admin || (user.Visible && user.Approved))
                    return user.Id;
                else
                    return 0;
            }
            else
                return 0;
        }

        public UserInfo GetCurrentUser()
        {
            UserInfo user = _context.Users.Find(_sessionContext.UserId);
            user.Password = "";
            return user;
        }

        public void ValidateClaim(ClaimsIdentity claimsIdentity)
        {
            var cookieClaim = claimsIdentity.FindFirst(ClaimTypes.Name);
            UserInfo user = _context.Users.FirstOrDefault(g => g.UserName == cookieClaim.Value);
            if (user != null)
            {
                _sessionContext.UserName = user.UserName;
                _sessionContext.UserId = user.Id;
            }
        }
    }
}
