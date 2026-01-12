using System;

namespace backend.Helpers
{
    public static class TokenHelper
    {
        public static string CreateTransferToken(int gameId, int newOwnerId)
        {
            string secret = Guid.NewGuid().ToString("N").Substring(0, 8);
            string payload = $"{gameId}:{newOwnerId}:{secret}";
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payload));
        }

        public static (int gameId, int newOwnerId, string secret)? DecodeTransferToken(string token)
        {
            try
            {
                var base64EncodedBytes = Convert.FromBase64String(token);
                var payload = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
                var parts = payload.Split(':');

                if (parts.Length != 3) return null;

                return (int.Parse(parts[0]), int.Parse(parts[1]), parts[2]);
            }
            catch
            {
                return null;
            }
        }
    }
}
