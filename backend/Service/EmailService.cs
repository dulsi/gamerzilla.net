using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using backend.Models;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using MailKit.Security;
using Microsoft.AspNetCore.Http;

namespace backend.Service
{

    public class EmailService
    {
        private readonly EmailSettings _settings;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EmailService(IOptions<EmailSettings> settings, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _settings = settings.Value;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        private async Task<SmtpClient> GetConnectedClientAsync()
        {
            var client = new SmtpClient();
            try
            {
                if (!_settings.ValidateSsl)
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                await client.ConnectAsync(_settings.SmtpServer, _settings.Port, SecureSocketOptions.Auto);

                if (!string.IsNullOrEmpty(_settings.Username))
                    await client.AuthenticateAsync(_settings.Username, _settings.Password);

                return client;
            }
            catch
            {
                client.Dispose();
                throw;
            }
        }

        
        public async Task SendApprovalRequestAsync(string newUsername, string adminEmail)
        {
            if (!_settings.Enabled) return;

            
            
            string baseUrl = GetBaseUrl();
            string adminLink = $"{baseUrl}/admin/users";

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            message.To.Add(new MailboxAddress("Admin", adminEmail));
            message.Subject = $"[Gamerzilla] Action Required: Approve User '{newUsername}'";

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = $@"
                <h3>New User Registration</h3>
                <p>User <strong>{newUsername}</strong> has registered and requires approval.</p>
                <p><a href='{adminLink}'>Go to Admin Dashboard</a> to approve or reject them.</p>";

            message.Body = bodyBuilder.ToMessageBody();

            using (var client = await GetConnectedClientAsync())
            {
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }


        public async Task SendPasswordResetAsync(string email, string username, string token)
        {
            if (!_settings.Enabled) return;

            
            string baseUrl = GetBaseUrl();
            string resetLink = $"{baseUrl}/reset-password?token={token}";

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            message.To.Add(new MailboxAddress(username, email));
            message.Subject = "[Gamerzilla] Reset your password";

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = $@"
        <h2>Hello {username},</h2>
        <p>We received a request to reset your Gamerzilla password.</p>
        <p>Please click the button below to choose a new password. This link will expire in 1 hour.</p>
        <p><a href='{resetLink}' style='padding:10px 15px; background-color:#007bff; color:white; text-decoration:none; border-radius:4px; display:inline-block;'>Reset Password</a></p>
        <p><small>If you did not request this, you can safely ignore this email.</small></p>
        <p><small>Direct link: {resetLink}</small></p>";

            message.Body = bodyBuilder.ToMessageBody();

            using (var client = await GetConnectedClientAsync())
            {
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }


        public async Task SendTransferVerificationEmailAsync(string email, string username, string newOwnerName, string gameName, string token)
        {
            if (!_settings.Enabled) return;

            
            string baseUrl = GetBaseUrl();
            
            string verifyLink = $"{baseUrl}/verify?token={token}";

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            message.To.Add(new MailboxAddress(username, email));
            message.Subject = $"[Gamerzilla] Confirm Transfer: {gameName}";

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = $@"
        <h2>Hello, {username},</h2>
        <p>You have requested to transfer ownership of the game <strong>{gameName}</strong> to user <strong>{newOwnerName}</strong>.</p>
        <p>This action cannot be undone. You will lose administrative rights to this game.</p>
        <p><a href='{verifyLink}' style='padding:10px 15px; background-color:#d9534f; color:white; text-decoration:none; border-radius:4px;'>Confirm Transfer</a></p>
        <p><small>Link: {verifyLink}</small></p>";

            message.Body = bodyBuilder.ToMessageBody();

            using (var client = await GetConnectedClientAsync())
            {
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }

        
        public async Task SendVerificationEmailAsync(string email, string username, string token)
        {
            if (!_settings.Enabled) return;

            
            

            string baseUrl = GetBaseUrl();
            string verifyLink = $"{baseUrl}/verify?token={token}";

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            message.To.Add(new MailboxAddress(username, email));
            message.Subject = "[Gamerzilla] Verify your account";

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = $@"
                <h2>Hello, {username}!</h2>
                <p>Please click the link below to verify your email address:</p>
                <p><a href='{verifyLink}' style='padding:10px 15px; background-color:#d9534f; color:white; text-decoration:none; border-radius:4px;'>Verify Email Address</a></p>
                <p><small>If the link above does not work, copy and paste this into your browser:<br>{verifyLink}</small></p>";

            message.Body = bodyBuilder.ToMessageBody();

            using (var client = await GetConnectedClientAsync())
            {
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }


        private string GetBaseUrl()
        {
            string frontend = _configuration["Frontend"];

            
            if (!string.IsNullOrWhiteSpace(frontend))
            {
                return frontend.Split(',')[0].TrimEnd('/');
            }

            
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request != null)
            {
                return $"{request.Scheme}://{request.Host}{request.PathBase}";
            }

            
            return "http://localhost:8080";
        }


    }
}