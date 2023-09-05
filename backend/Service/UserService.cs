using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using backend.Dto;
using backend.Models;
using backend.Context;
using backend.Settings;

namespace backend.Service;

public class UserService
{
    SessionContext _sessionContext;
    UserContext _context;
    private readonly RegistrationOptions _options;
    ILogger<UserService> _log;
    IMapper _mapper { get; }

    public UserService(SessionContext sessionContext, UserContext userContext, IOptions<RegistrationOptions> options, ILogger<UserService> log, IMapper mapper)
    {
        _sessionContext = sessionContext;
        _context = userContext;
        _context.Database.EnsureCreated();
        _options = options.Value;
        _log = log;
        _mapper = mapper;
    }

    public bool Approve(string userName)
    {
        UserInfo user = _context.Users.FirstOrDefault(u => u.UserName == userName);
        if (user == null)
            return false;
        user.Approved = true;
        _context.SaveChanges();
        return true;
    }

    public IEnumerable<UserInfoDto> GetUsers(bool admin)
    {
        IEnumerable<UserInfoDto> res;
        if (admin)
        {
            res = _mapper.Map<IEnumerable<UserInfoDto>>(_context.Users.ToList());
            foreach (UserInfoDto i in res)
            {
                i.password = "";
                i.canApprove = false;
            }
        }
        else
        {
            res = _mapper.Map<IEnumerable<UserInfoDto>>(_context.Users.Where(u => u.Visible == true && u.Approved == true).ToList());
            foreach (UserInfoDto i in res)
            {
                i.password = "";
                i.admin = false;
                i.canApprove = false;
            }
        }
        return res;
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

    public UserInfoDto GetCurrentUser()
    {
        UserInfoDto user = _mapper.Map<UserInfoDto>(_context.Users.Find(_sessionContext.UserId));
        user.password = "";
        user.canApprove = false;
        return user;
    }
    
    public async Task<UserInfo> RegisterUser(string userName, string password)
    {
        UserInfo user = new UserInfo();
        user.UserName = userName;
        user.Password = password;
        user.Approved = !_options.RequireApproval;
        user.Visible = false;
        user.Admin = false;
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task ValidateClaim(ClaimsIdentity claimsIdentity)
    {
        var cookieClaim = claimsIdentity.FindFirst(ClaimTypes.Name);
        UserInfo user = await _context.Users.FirstOrDefaultAsync(g => g.UserName == cookieClaim.Value);
        if (user != null)
        {
            _sessionContext.UserName = user.UserName;
            _sessionContext.UserId = user.Id;
        }
    }

    public bool Visible(string userName, int val)
    {
        UserInfo user = _context.Users.FirstOrDefault(u => u.UserName == userName);
        if (user == null)
            return false;
        user.Visible = (val == 1);
        _context.SaveChanges();
        return true;
    }
}
