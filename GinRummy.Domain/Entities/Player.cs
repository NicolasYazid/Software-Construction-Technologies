using System;

using GinRummy.Domain.Dtos;

namespace GinRummy.Domain.Entities
{
    public class Player
    {
        // These are the same limits as the username and email columns of the Player table.
        // They are public so that input validation can check them before an entity is ever built.
        public const int MaxUsernameLength = 20;
        public const int MaxEmailLength = 254;

        public Player(NewPlayerDto newPlayer)
        {
            ValidateNewPlayer(newPlayer);
            ValidateUsername(newPlayer.Username);
            ValidateEmail(newPlayer.Email);
            ValidatePasswordHash(newPlayer.PasswordHash);
            ValidateLocaleId(newPlayer.LocaleId);

            Username = newPlayer.Username.Trim();
            Email = newPlayer.Email.Trim().ToLowerInvariant();
            PasswordHash = newPlayer.PasswordHash;
            LocaleId = newPlayer.LocaleId;
            CreatedAt = DateTime.UtcNow;
            IsEmailVerified = false;
        }

        // Entity Framework needs a parameterless constructor to materialize rows.
        // It is protected so that application code cannot skip the validating constructor.
        protected Player()
        {
        }

        public int PlayerId { get; private set; }
        // Always PlayerId + 100000, computed by SQL Server and never assigned by the application.
        public int PublicTag { get; private set; }
        public string Username { get; private set; }
        // Stored in lower case so that two spellings of the same address never become two accounts.
        public string Email { get; private set; }
        public string PasswordHash { get; private set; }
        public int LocaleId { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? LastLoginAt { get; private set; }
        public bool IsEmailVerified { get; private set; }

        private static void ValidateNewPlayer(NewPlayerDto newPlayer)
        {
            if (newPlayer == null)
            {
                throw new ArgumentNullException(nameof(newPlayer));
            }
        }

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

        // The email format is checked by input validation before construction, so only the database limits are guarded here.
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

        private static void ValidateLocaleId(int localeId)
        {
            if (localeId <= 0)
            {
                throw new ArgumentException("Locale id must be positive.", nameof(localeId));
            }
        }
    }
}
