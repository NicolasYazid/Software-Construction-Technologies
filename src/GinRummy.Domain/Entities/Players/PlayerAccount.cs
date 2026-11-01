using System;

namespace GinRummy.Domain.Entities.Players
{
    /// <summary>
    /// The account of a registered player, mapped to the PlayerAccount table: the
    /// credentials and the state of the account. It shares the key of its player, since a
    /// player has at most one account. Protects its own invariants: an account cannot exist
    /// without its player, a valid email and a password hash.
    /// </summary>
    public class PlayerAccount
    {
        /// <summary>
        /// Length of the email column.
        /// </summary>
        public const int EmailMaxLength = 254;

        /// <summary>
        /// Length of the password column, which stores the hash.
        /// </summary>
        public const int PasswordHashMaxLength = 255;

        /// <summary>
        /// Creates a valid new account for a player that is already stored. A new account
        /// always starts unverified and without two-step verification.
        /// </summary>
        /// <param name="playerId">Identifier of the player that owns the account; must be positive.</param>
        /// <param name="email">Account email; required, at most 254 characters.</param>
        /// <param name="passwordHash">Hash of the password; required, never plain text.</param>
        public PlayerAccount(int playerId, string email, string passwordHash)
        {
            ValidatePlayerId(playerId);
            ValidateEmail(email);
            ValidatePasswordHash(passwordHash);

            PlayerId = playerId;
            Email = NormalizeEmail(email);
            PasswordHash = passwordHash;
            IsEmailVerified = false;
            IsTwoFactorEnabled = false;
        }

        /// <summary>
        /// Parameterless constructor reserved for Entity Framework's materialization; not
        /// for application code, which must use the validating constructor.
        /// </summary>
        protected PlayerAccount()
        {
        }

        /// <summary>
        /// Gets the identifier of the player that owns the account, which is also the key
        /// of the account.
        /// </summary>
        public int PlayerId { get; private set; }

        /// <summary>
        /// Gets the account email address, always stored in lower case.
        /// </summary>
        public string Email { get; private set; }

        /// <summary>
        /// Gets the hash of the password. Never the plain text password.
        /// </summary>
        public string PasswordHash { get; private set; }

        /// <summary>
        /// Gets the last sign-in date, in UTC. Null until the first sign-in.
        /// </summary>
        public DateTime? LastLoginAt { get; private set; }

        /// <summary>
        /// Gets whether the email address has already been verified.
        /// </summary>
        public bool IsEmailVerified { get; private set; }

        /// <summary>
        /// Gets whether signing in also requires a code sent by email.
        /// </summary>
        public bool IsTwoFactorEnabled { get; private set; }

        /// <summary>
        /// Gets the player that owns the account, loaded from the relationship between the
        /// two tables.
        /// </summary>
        public virtual Player Player { get; private set; }

        /// <summary>
        /// Marks the email address as verified.
        /// </summary>
        public void MarkEmailVerified()
        {
            IsEmailVerified = true;
        }

        /// <summary>
        /// Records the moment of the last successful sign-in.
        /// </summary>
        /// <param name="loggedInAt">Moment the player signed in, in UTC.</param>
        public void RegisterLogin(DateTime loggedInAt)
        {
            LastLoginAt = loggedInAt;
        }

        /// <summary>
        /// Replaces the password hash.
        /// </summary>
        /// <param name="passwordHash">Hash of the new password; required, never plain text.</param>
        public void ChangePasswordHash(string passwordHash)
        {
            ValidatePasswordHash(passwordHash);

            PasswordHash = passwordHash;
        }

        /// <summary>
        /// Replaces the email address. The new address was already proven to belong to the
        /// player, so the account stays verified.
        /// </summary>
        /// <param name="email">New account email; required, at most 254 characters.</param>
        public void ChangeEmail(string email)
        {
            ValidateEmail(email);

            Email = NormalizeEmail(email);
        }

        /// <summary>
        /// Turns on two-step verification.
        /// </summary>
        public void EnableTwoFactor()
        {
            IsTwoFactorEnabled = true;
        }

        /// <summary>
        /// Turns off two-step verification.
        /// </summary>
        public void DisableTwoFactor()
        {
            IsTwoFactorEnabled = false;
        }

        // The key of the account is the key of a player that already exists.
        private static void ValidatePlayerId(int playerId)
        {
            if (playerId <= 0)
            {
                throw new ArgumentException("Player id must be positive.", nameof(playerId));
            }
        }

        // An account must always have a non-empty email within the length the database column allows.
        // The format is checked as input validation before the account is built.
        private static void ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email is required.", nameof(email));
            }

            if (email.Trim().Length > EmailMaxLength)
            {
                throw new ArgumentException("Email is too long.", nameof(email));
            }
        }

        // An account can never be stored without a password hash.
        private static void ValidatePasswordHash(string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(passwordHash))
            {
                throw new ArgumentException("Password hash is required.", nameof(passwordHash));
            }
        }

        // The column is unique, so the same address in different case must be the same value.
        private static string NormalizeEmail(string email)
        {
            string normalizedEmail = email.Trim().ToLowerInvariant();

            return normalizedEmail;
        }
    }
}
