using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GinRummy.Domain.Entities
{
    /// <summary>
    /// The account of a registered player, mapped to the PlayerAccount table: the
    /// credentials and the state of the account. It shares the key of its player, since a
    /// player has at most one account. Protects its own invariants: an account cannot exist
    /// without its player, a valid email and a password hash.
    /// </summary>
    public class PlayerAccount
    {
        private const int MaxEmailLength = 254;

        /// <summary>
        /// Creates a valid new account for a player that is already stored. A new account
        /// always starts unverified.
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
            Email = email.Trim().ToLowerInvariant();
            PasswordHash = passwordHash;
            IsEmailVerified = false;
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
        /// Gets the player that owns the account, loaded from the relationship between the
        /// two tables.
        /// </summary>
        public virtual Player Player { get; private set; }

        // The key of the account is the key of a player that already exists.
        private static void ValidatePlayerId(int playerId)
        {
            if (playerId <= 0)
            {
                throw new ArgumentException("Player id must be positive.", nameof(playerId));
            }
        }

        // An account must always have a non-empty email within the length the database
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

        // An account can never be stored without a password hash.
        private static void ValidatePasswordHash(string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(passwordHash))
            {
                throw new ArgumentException("Password hash is required.", nameof(passwordHash));
            }
        }
    }
}
