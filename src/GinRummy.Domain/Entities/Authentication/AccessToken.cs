using System;

namespace GinRummy.Domain.Entities.Authentication
{
    public class AccessToken
    {
        public AccessToken(string value, DateTime expiresAt)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("The token value is required.", nameof(value));
            }

            Value = value;
            ExpiresAt = expiresAt;
        }

        public string Value { get; private set; }

        public DateTime ExpiresAt { get; private set; }
    }
}
