namespace backend.Models
{
    public class EmailSettings
    {
        public bool Enabled { get; set; }
        public string SmtpServer { get; set; }
        public int Port { get; set; }
        public string SenderName { get; set; }
        public string SenderEmail { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool ValidateSsl { get; set; } = true;
    }
}
