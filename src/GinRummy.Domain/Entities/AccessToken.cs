using System;

namespace GinRummy.Domain.Entities
{
    /// <summary>
    /// Signed proof that a player signed in, which the client presents in every later request. It is not
    /// stored on the server: its signature and its expiry are enough to check it.
    /// </summary>
    public class AccessToken
    {
        /// <summary>
        /// Builds an access token.
        /// </summary>
        /// <param name="value">Encoded token, with its claims and its signature.</param>
        /// <param name="expiresAt">Moment the token stops being valid, in UTC.</param>
        /// <exception cref="ArgumentException">Thrown when the value is empty.</exception>
        public AccessToken(string value, DateTime expiresAt)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("The token value is required.", nameof(value));
            }

            Value = value;
            ExpiresAt = expiresAt;
        }

        /// <summary>
        /// Gets the encoded token, with its claims and its signature.
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// Gets the moment the token stops being valid, in UTC.
        /// </summary>
        public DateTime ExpiresAt { get; private set; }
    }
}
