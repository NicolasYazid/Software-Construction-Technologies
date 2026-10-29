using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GinRummy.Domain.Entities
{
    /// <summary>
    /// A one-time code sent to confirm an email address or authorize a sensitive action.
    /// Protects its own invariants: it cannot expire before it was created, it always
    /// starts with zero attempts and unused, and it can be used only once.
    /// </summary>
    public class VerificationCode
    {
        /// <summary>
        /// Fixed length of the code_hash column: the hexadecimal text of a SHA-256 hash.
        /// </summary>
        public const int CodeHashLength = 64;

        /// <summary>
        /// Creates a valid new verification code, stamped with the current UTC time. Kept for
        /// callers that have no clock yet; code that has one should pass the moment explicitly.
        /// </summary>
        /// <param name="playerId">Player this code is for; must be positive.</param>
        /// <param name="purpose">Reason this code was generated.</param>
        /// <param name="codeHash">Hash of the code; required, never the plain code.</param>
        /// <param name="expiresAt">When the code stops being valid; must be in the future.</param>
        public VerificationCode(int playerId, VerificationPurpose purpose, string codeHash, DateTime expiresAt)
            : this(playerId, purpose, codeHash, DateTime.UtcNow, expiresAt)
        {
        }

        /// <summary>
        /// Creates a valid new verification code generated at the given moment.
        /// </summary>
        /// <param name="playerId">Player this code is for; must be positive.</param>
        /// <param name="purpose">Reason this code was generated.</param>
        /// <param name="codeHash">Hash of the code; required, never the plain code.</param>
        /// <param name="createdAt">Moment the code was generated, in UTC.</param>
        /// <param name="expiresAt">When the code stops being valid; must be after the creation.</param>
        public VerificationCode(
            int playerId,
            VerificationPurpose purpose,
            string codeHash,
            DateTime createdAt,
            DateTime expiresAt)
        {
            ValidatePlayerId(playerId);
            ValidateCodeHash(codeHash);
            ValidateExpiry(expiresAt, createdAt);

            PlayerId = playerId;
            Purpose = purpose;
            CodeHash = codeHash;
            ExpiresAt = expiresAt;
            CreatedAt = createdAt;
            Attempts = 0;
            UsedAt = null;
        }

        /// <summary>
        /// Parameterless constructor reserved for Entity Framework's materialization; not
        /// for application code, which must use the validating constructor.
        /// </summary>
        protected VerificationCode()
        {
        }

        /// <summary>
        /// Gets the internal, autonumeric identifier.
        /// </summary>
        public int VerificationCodeId { get; private set; }

        /// <summary>
        /// Gets the player this code was generated for.
        /// </summary>
        public int PlayerId { get; private set; }

        /// <summary>
        /// Gets the reason this code was generated.
        /// </summary>
        public VerificationPurpose Purpose { get; private set; }

        /// <summary>
        /// Gets the hash of the code, hex-encoded. Never the plain code itself.
        /// </summary>
        public string CodeHash { get; private set; }

        /// <summary>
        /// Gets the moment this code stops being valid, in UTC.
        /// </summary>
        public DateTime ExpiresAt { get; private set; }

        /// <summary>
        /// Gets the moment this code was successfully used, in UTC. Null while unused.
        /// </summary>
        public DateTime? UsedAt { get; private set; }

        /// <summary>
        /// Gets how many times a wrong code has been entered against this one.
        /// </summary>
        public int Attempts { get; private set; }

        /// <summary>
        /// Gets the moment this code was generated, in UTC.
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// Counts one more wrong code entered against this one.
        /// </summary>
        public void RegisterFailedAttempt()
        {
            Attempts++;
        }

        /// <summary>
        /// Marks the code as used so it cannot confirm anything else.
        /// </summary>
        /// <param name="usedAt">Moment the code was successfully entered, in UTC.</param>
        /// <exception cref="InvalidOperationException">Thrown when the code was already used.</exception>
        public void MarkAsUsed(DateTime usedAt)
        {
            if (UsedAt.HasValue)
            {
                throw new InvalidOperationException("The verification code was already used.");
            }

            UsedAt = usedAt;
        }

        // A code must always belong to a real player, referenced by a positive id.
        private static void ValidatePlayerId(int playerId)
        {
            if (playerId <= 0)
            {
                throw new ArgumentException("Player id must be positive.", nameof(playerId));
            }
        }

        // A code is never stored in plain form, so its hash can never be empty.
        private static void ValidateCodeHash(string codeHash)
        {
            if (string.IsNullOrWhiteSpace(codeHash))
            {
                throw new ArgumentException("Code hash is required.", nameof(codeHash));
            }
        }

        // A code that expires at or before the moment it was created would be born
        // already invalid, which must never happen.
        private static void ValidateExpiry(DateTime expiresAt, DateTime creationMoment)
        {
            if (expiresAt <= creationMoment)
            {
                throw new ArgumentException("Expiry must be after creation.", nameof(expiresAt));
            }
        }
    }
}
