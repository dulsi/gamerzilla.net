﻿using AutoMapper;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using backend.Dto;
using backend.Models;
using backend.Filters;
using backend.Context;
using backend.Service;
using backend.Settings;

namespace backend.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly UserContext _context;
        private readonly SessionContext _sessionContext;
        private readonly UserService _userService;
        private readonly RegistrationOptions _options;
        private IMapper _mapper { get; }

        public UserController(ILogger<UserController> logger, UserContext context, SessionContext sessionContext, UserService userService, IOptions<RegistrationOptions> options, IMapper mapper)
        {
            _logger = logger;
            _context = context;
            _context.Database.EnsureCreated();
            _sessionContext = sessionContext;
            _userService = userService;
            _options = options.Value;
            _mapper = mapper;
        }

        [Authorize]
        [AllowAnonymous]
        public IEnumerable<UserInfoDto> GetUsers()
        {
            bool admin = false;
            try
            {
                _userService.ValidateClaim(User.Identity as ClaimsIdentity);
                admin = _userService.GetCurrentUser().admin;
            }
            catch (System.Exception) { }
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

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginInfo login)
        {
            if (!_userService.IsValidUser(login.username, login.password))
                return BadRequest();

            var claimsIdentity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, login.username),
            }, "Cookies");

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            await Request.HttpContext.SignInAsync("Cookies", claimsPrincipal);
            _userService.ValidateClaim(claimsIdentity);
            return Ok(_userService.GetCurrentUser());
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] LoginInfo login)
        {
            if (_options.Allow == false)
                return BadRequest();
            var u = _userService.RegisterUser(login.username, login.password);
            return Ok(u);
        }

        [HttpGet]
        [Route("canregister")]
        public async Task<IActionResult> CabRegister()
        {
            return Ok(_options.Allow);
        }

        [Authorize]
        [Route("whoami")]
        public async Task<IActionResult> WhoAmI()
        {
            _userService.ValidateClaim(User.Identity as ClaimsIdentity);
            return Ok(_userService.GetCurrentUser());
        }


        [Authorize]
        [Route("approve")]
        public async Task<IActionResult> Approve(string username)
        {
            bool admin = false;
            try
            {
                _userService.ValidateClaim(User.Identity as ClaimsIdentity);
                admin = _userService.GetCurrentUser().admin;
            }
            catch (System.Exception) { }
            if (!admin)
                return BadRequest();
            UserInfo user = _context.Users.FirstOrDefault(u => u.UserName == username);
            if (user == null)
                return BadRequest();
            user.Approved = true;
            _context.SaveChanges();
            return Ok(true);
        }
    }

    public class LoginInfo
    {
        public string username { get; set; }
        public string password { get; set; }
    }
}
