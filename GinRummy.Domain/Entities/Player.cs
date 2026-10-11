using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GinRummy.Domain.Entities
{
    /// <summary>
    /// A registered player account, mapped to the Player table. Protects its own
    /// invariants: a player cannot exist without a valid username, email and password hash.
    /// </summary>
    public class Player
    {
        private const int MaxUsernameLength = 20;
        private const int MaxEmailLength = 254;

        /// <summary>
        /// Creates a valid new player. A new account always starts unverified and stamped
        /// with the current UTC time.
        /// </summary>
        /// <param name="username">Display name; required, at most 20 characters.</param>
        /// <param name="email">Account email; required, at most 254 characters.</param>
        /// <param name="passwordHash">Hash of the password; required, never plain text.</param>
        /// <param name="localeId">Locale chosen for the account; must be positive.</param>
        public Player(string username, string email, string passwordHash, int localeId)
        {
            ValidateUsername(username);
            ValidateEmail(email);
            ValidatePasswordHash(passwordHash);
            ValidateLocaleId(localeId);

            Username = username.Trim();
            Email = email.Trim().ToLowerInvariant();
            PasswordHash = passwordHash;
            LocaleId = localeId;
            CreatedAt = DateTime.UtcNow;
            IsEmailVerified = false;
        }

        /// <summary>
        /// Parameterless constructor reserved for Entity Framework's materialization; not
        /// for application code, which must use the validating constructor.
        /// </summary>
        protected Player()
        {
        }

        /// <summary>
        /// Gets the internal, autonumeric identifier.
        /// </summary>
        public int PlayerId { get; private set; }

        /// <summary>
        /// Gets the public identifier shown to other players, always PlayerId + 100000.
        /// Computed by SQL Server; never assigned by the application.
        /// </summary>
        public int PublicTag { get; private set; }

        /// <summary>
        /// Gets the display name chosen at registration.
        /// </summary>
        public string Username { get; private set; }

        /// <summary>
        /// Gets the account email address, always stored in lower case.
        /// </summary>
        public string Email { get; private set; }

        /// <summary>
        /// Gets the hash of the password. Never the plain text password.
        /// </summary>
        public string PasswordHash { get; private set; }

        /// <summary>
        /// Gets the locale configured for the account.
        /// </summary>
        public int LocaleId { get; private set; }

        /// <summary>
        /// Gets the registration date, in UTC.
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// Gets the last sign-in date, in UTC. Null until the first sign-in.
        /// </summary>
        public DateTime? LastLoginAt { get; private set; }

        /// <summary>
        /// Gets whether the email address has already been verified.
        /// </summary>
        public bool IsEmailVerified { get; private set; }

        // A player must always have a non-empty username within the length the database
        // column allows.
        private static void ValidateUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username is required.", nameof(username));
            }

            if (username.Trim().Length > MaxUsernameLength)
            {
                throw new ArgumentException("Username is too long.", nameof(username));
            }
        }

        // A player must always have a non-empty email within the length the database
        // column allows. Format is checked as input validation before construction.
        private static void ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email is required.", nameof(email));
            }

            if (email.Trim().Length > MaxEmailLength)
            {
                throw new ArgumentException("Email is too long.", nameof(email));
            }
        }

        // A player can never be stored without a password hash.
        private static void ValidatePasswordHash(string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(passwordHash))
            {
                throw new ArgumentException("Password hash is required.", nameof(passwordHash));
            }
        }

        // The locale is a foreign key, so it must reference a real, positive id.
        private static void ValidateLocaleId(int localeId)
        {
            if (localeId <= 0)
            {
                throw new ArgumentException("Locale id must be positive.", nameof(localeId));
            }
        }
    }
}
