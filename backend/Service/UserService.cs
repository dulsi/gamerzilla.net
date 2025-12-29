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
using backend.Helpers;
using backend.Settings;
using System;

namespace backend.Service;

public class UserService
{
    SessionContext _sessionContext;
    GamerzillaContext _context;
    private readonly RegistrationOptions _options;
    private readonly EmailService _emailService;
    ILogger<UserService> _log;
    private string _cachedHtml = null;


    public UserService(SessionContext sessionContext, GamerzillaContext userContext, IOptions<RegistrationOptions> options, ILogger<UserService> log, EmailService emailService)
    {
        _sessionContext = sessionContext;
        _context = userContext;
        _options = options.Value;
        _log = log;
        _emailService = emailService;
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

    public async Task<UserInfoDto> GetUserByUsernameAsync(string userName)
    {
        var u = await _context.Users.FirstOrDefaultAsync(u => u.UserName.ToLower() == userName.ToLower());

        if (u == null) return null;

        return new UserInfoDto
        {
            userName = u.UserName,
            email = u.Email,
            admin = u.Admin,
            approved = u.Approved,
            visible = u.Visible,
            password = "", 
            canApprove = u.Approved
        };
    }

    public IEnumerable<UserInfoDto> GetUsers(bool admin)
    {
        if (admin)
        {
            
            return _context.Users
                .Select(u => new UserInfoDto
                {
                    userName = u.UserName,
                    email = u.Email,
                    admin = u.Admin,
                    approved = u.Approved,
                    visible = u.Visible,
                    password = "",
                    canApprove = u.Approved
                })
                .ToList();
        }
        else
        {
            
            return _context.Users
                .Where(u => u.Visible == true &&
                       (u.Approved == true || u.Admin == true || !_options.RequireApproval))
                .Select(u => new UserInfoDto
                {
                    userName = u.UserName,
                    email = "",
                    admin = false,
                    approved = u.Approved,
                    visible = u.Visible,
                    password = "",

                    
                    canApprove = false
                })
                .ToList();
        }
    }




    public bool IsValidUser(string userName, string password)
    {
        
        UserInfo user = _context.Users.FirstOrDefault(g => g.UserName.ToLower() == userName.ToLower());

        if (user != null)
        {
            
            if (user.Password == password)
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(password);
                _context.SaveChanges();

                _sessionContext.UserName = userName;
                _sessionContext.UserId = user.Id;
                return true;
            }

            
            try
            {
                if (BCrypt.Net.BCrypt.Verify(password, user.Password))
                {
                    _sessionContext.UserName = userName;
                    _sessionContext.UserId = user.Id;
                    return true;
                }
            }
            catch { /* Ignore invalid hash formats */ }

            
            return false;
        }

        
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
        UserInfo user = _context.Users.FirstOrDefault(g => g.UserName.ToLower() == userName.ToLower());
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
        var dbUser = _context.Users.Find(_sessionContext.UserId);


        if (dbUser == null) return null;

        return new UserInfoDto
        {
            id = dbUser.Id,
            userName = dbUser.UserName,
            email = dbUser.Email,
            admin = dbUser.Admin,
            approved = dbUser.Approved,
            visible = dbUser.Visible,
            password = "",
            canApprove = dbUser.Admin
        };
    }



    public async Task<UserInfo> RegisterUser(string userName, string password, string email)
    {
        
        if (!PasswordHelper.IsStrongEnough(password))
        {
            throw new Exception("Password is too weak. It must be at least 'Fair' strength.");
        }

        
        var duplicates = await _context.Users
            .Where(u => u.UserName.ToLower() == userName.ToLower() ||
                       (!string.IsNullOrEmpty(email) && u.Email.ToLower() == email.ToLower()))
            .Select(u => new { u.UserName, u.Email })
            .ToListAsync();

        if (duplicates.Any())
        {
            if (duplicates.Any(u => u.UserName.ToLower() == userName.ToLower()))
                throw new Exception("User exists");
            throw new Exception("Email in use");
        }

        
        bool isFirstUser = !await _context.Users.AnyAsync();



        UserInfo user = new UserInfo();
        user.UserName = userName;
        user.Password = BCrypt.Net.BCrypt.HashPassword(password); 
        user.Email = email;
        user.Visible = true;

        if (isFirstUser)
        {
            
            user.Admin = true;
            user.Approved = true;
            user.Email = email;

            
            
        }
        else
        {
            
            user.Admin = false;

            if (_options.RequireApproval)
            {
                user.Approved = false; 

                
                if (!string.IsNullOrEmpty(email) && _options.RequireEmailVerification)
                {
                    user.VerificationToken = Guid.NewGuid().ToString();
                    user.TokenExpiration = DateTime.UtcNow.AddHours(24);

                    
                    _ = Task.Run(() => _emailService.SendVerificationEmailAsync(email, userName, user.VerificationToken));
                }
                
                else
                {
                    
                    var admins = await _context.Users
                        .Where(u => u.Admin == true && !string.IsNullOrEmpty(u.Email))
                        .ToListAsync();

                    if (admins.Any())
                    {
                        foreach (var admin in admins)
                        {
                            _ = Task.Run(() => _emailService.SendApprovalRequestAsync(userName, admin.Email));
                        }
                    }
                    else
                    {
                        
                        
                        System.Console.WriteLine($"[Warning] User {userName} registered, but no Admin emails found to notify!");
                    }
                }
            }
            else
            {
                user.Approved = true; 
            }
        }

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }




    public async Task<TokenProcessResult> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) throw new TokenException("User not found.");

        
        if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.Password))
        {
            throw new TokenException("Incorrect old password.");
        }

        if (!PasswordHelper.IsStrongEnough(newPassword))
        {
            throw new TokenException("New password is not strong enough.");
        }

        
        user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _context.SaveChangesAsync();

        return new TokenProcessResult
        {
            Message = "Your password has been changed successfully.",
            ActionType = "Password"
        };
    }

    public async Task<string> RequestTokenAsync(string emailOrUsername)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u =>
            u.UserName.ToLower() == emailOrUsername.ToLower() ||
            u.Email.ToLower() == emailOrUsername.ToLower());

        if (user == null) return "Success"; 

        
        if (user.TokenExpiration > DateTime.UtcNow.AddMinutes(55)) return "Wait";

        user.VerificationToken = Guid.NewGuid().ToString("N");

        if (!user.Approved)
        {
            
            user.TokenExpiration = DateTime.UtcNow.AddHours(24);
            await _context.SaveChangesAsync();
            await _emailService.SendVerificationEmailAsync(user.Email, user.UserName, user.VerificationToken);
        }
        else
        {
            
            user.TokenExpiration = DateTime.UtcNow.AddHours(1);
            user.PendingEmail = null; 
            await _context.SaveChangesAsync();
            await _emailService.SendPasswordResetAsync(user.Email, user.UserName, user.VerificationToken);
        }
        return "Success";
    }


    public async Task<string> PromoteToAdminAsync(string username)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName.ToLower() == username.ToLower());

        if (user == null) return "User not found.";

        
        if (user.Admin) return "User is already an admin.";

        user.Admin = true;
        await _context.SaveChangesAsync();
        return "Success";
    }


    public async Task<string> DemoteAdminAsync(string username, string currentLoggedInUser)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName.ToLower() == username.ToLower());

        if (user == null) return "User not found.";

        
        if (username.ToLower() == currentLoggedInUser.ToLower())
            return "You cannot revoke your own admin status.";

        if (!user.Admin) return "User is not an admin.";

        user.Admin = false;
        await _context.SaveChangesAsync();
        return "Success";
    }


    public async Task<bool> DeleteUserAsync(string username)
    {
        
        var userToDelete = await _context.Users.FirstOrDefaultAsync(u => u.UserName.ToLower() == username.ToLower());

        if (userToDelete == null) return false;

        
        _context.Users.Remove(userToDelete);
        await _context.SaveChangesAsync();

        return true;
    }


    public async Task<TokenProcessResult> ProcessTokenAsync(string token, string newPassword = null)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.VerificationToken == token);

        if (user == null || user.TokenExpiration <= DateTime.UtcNow)
            throw new TokenException("Invalid or expired token.");

        
        if (!string.IsNullOrEmpty(newPassword))
        {
            if (!PasswordHelper.IsStrongEnough(newPassword))
                throw new TokenException("Password too weak.");

            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.PendingEmail = null;
            await SaveAndCleanup(user);
            return new TokenProcessResult { Message = "Password reset successfully!", ActionType = "Password" };
        }

        
        var transferData = TokenHelper.DecodeTransferToken(token);
        if (transferData != null)
        {
            var (gameId, newOwnerId, _) = transferData.Value;
            if (user.Id == newOwnerId)
            {
                var game = await _context.Games.FindAsync(gameId);
                if (game != null)
                {
                    game.OwnerId = newOwnerId;
                    await SaveAndCleanup(user);
                    return new TokenProcessResult { Message = "Game transfer accepted!", ActionType = "Transfer" };
                }
            }
        }

        
        if (!string.IsNullOrEmpty(user.PendingEmail))
        {
            user.Email = user.PendingEmail;
            user.PendingEmail = null;
            await SaveAndCleanup(user);
            return new TokenProcessResult { Message = "Email updated successfully!", ActionType = "Email" };
        }

        
        
        
        await SaveAndCleanup(user);
        return new TokenProcessResult
        {
            Message = "Your account has been verified successfully! You can now log in.",
            ActionType = "Registration"
        };
    }

    private async Task SaveAndCleanup(UserInfo user)
    {
        user.Approved = true;
        user.VerificationToken = null;
        user.TokenExpiration = null;
        await _context.SaveChangesAsync();
    }


    //public async Task<string> ProcessTokenAsync(string token, string newPassword = null)
    //{
    

    
    

    
    
    
    
    
    
    
    
    
    
    
    
    

    
    
    
    

    
    
    //}

public async Task<TokenProcessResult> RequestEmailUpdateAsync(int userId, string newEmail)
{
    
    if (await _context.Users.AnyAsync(u => u.Email == newEmail))
    {
        throw new TokenException("Email is already in use.");
    }

    var user = await _context.Users.FindAsync(userId);
    if (user == null) throw new TokenException("User not found.");

    
    user.PendingEmail = newEmail;
    user.VerificationToken = Guid.NewGuid().ToString();
    user.TokenExpiration = DateTime.UtcNow.AddHours(24);

    await _context.SaveChangesAsync();

    
    await _emailService.SendVerificationEmailAsync(newEmail, user.UserName, user.VerificationToken);

    return new TokenProcessResult 
    { 
        Message = "A verification email has been sent to your new address.", 
        ActionType = "Email" 
    };
}


    //public async Task<bool> VerifyEmailToken(string token)
    //{
    

    
    
    
    
    
    
    
    
    
    //}


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
