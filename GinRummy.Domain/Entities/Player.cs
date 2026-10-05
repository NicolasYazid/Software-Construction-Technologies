using System;

namespace GinRummy.Domain.Entities
{
    // A registered player account, mapped to the Player table. It protects its own invariants:
    // a player cannot exist without a valid username, email and password hash.
    public class Player
    {
        // The same limits as the username and email columns of the Player table, public so that
        // input validation can check them before an entity is ever built.
        public const int MaxUsernameLength = 20;
        public const int MaxEmailLength = 254;

        // A new account always starts unverified and stamped with the current UTC time.
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

        // Reserved for the materialization of Entity Framework; application code goes through
        // the validating constructor.
        protected Player()
        {
        }

        public int PlayerId { get; private set; }
        // Always PlayerId + 100000, computed by SQL Server and never assigned by the application.
        public int PublicTag { get; private set; }
        public string Username { get; private set; }
        // Stored in lower case so that two spellings of the same address never become two accounts.
        public string Email { get; private set; }
        // The Argon2id hash in PHC format, never the plain text password.
        public string PasswordHash { get; private set; }
        public int LocaleId { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? LastLoginAt { get; private set; }
        public bool IsEmailVerified { get; private set; }

        // A player must always have a non-empty username within the length the database column
        // allows.
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

        // A player must always have a non-empty email within the length the database column
        // allows. Its format is checked as input validation before construction.
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
