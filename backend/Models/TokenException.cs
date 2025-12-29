using System;

namespace backend.Models
{
    public class TokenException : Exception
    {
        public TokenException(string message) : base(message) { }
    }
}
