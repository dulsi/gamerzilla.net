using System.Linq;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using backend.Models;
using backend.Context;
using backend.Settings;

namespace backend.Service
{
    public class UserService
    {
        SessionContext _sessionContext;
        UserContext _context;
        private readonly RegistrationOptions _options;
        ILogger<UserService> _log;

        public UserService(SessionContext sessionContext, UserContext userContext, IOptions<RegistrationOptions> options, ILogger<UserService> log)
        {
            _sessionContext = sessionContext;
            _context = userContext;
            _context.Database.EnsureCreated();
            _options = options.Value;
            _log = log;
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
            {
                if (!_context.Users.Any())
                {
                    if (_options.AdminUsername != "" && _options.AdminPassword != "")
                    {
                        user = new UserInfo();
                        user.UserName = _options.AdminUsername;
                        user.Password = _options.AdminPassword;
                        user.Approved = true;
                        user.Visible = true;
                        user.Admin = true;
                        _context.Users.Add(user);
                        _context.SaveChanges();
                        if (userName == _options.AdminUsername && password == _options.AdminPassword)
                            return IsValidUser(userName, password);
                    }
                }
                return false;
            }
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
            {
                return 0;
            }
        }

        public UserInfo GetCurrentUser()
        {
            UserInfo user = _context.Users.Find(_sessionContext.UserId);
            user.Password = "";
            return user;
        }
        
        public UserInfo RegisterUser(string userName, string password)
        {
            UserInfo user = new UserInfo();
            user.UserName = userName;
            user.Password = password;
            user.Approved = !_options.RequireApproval;
            user.Visible = false;
            user.Admin = false;
            _context.Users.Add(user);
            _context.SaveChanges();
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
