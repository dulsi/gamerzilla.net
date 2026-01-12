using System.Linq;

namespace backend.Helpers
{
    public static class PasswordHelper
    {
        public static bool IsStrongEnough(string password)
        {
            if (string.IsNullOrEmpty(password)) return false;

            int score = 0;
            if (password.Length > 7) score++;
            if (password.Length > 10) score++;
            if (password.Any(char.IsUpper)) score++;
            if (password.Any(char.IsDigit)) score++;
            if (password.Any(ch => !char.IsLetterOrDigit(ch))) score++;


            return score >= 2;
        }
    }
}