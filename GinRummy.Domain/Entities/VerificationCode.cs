using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GinRummy.Domain.Entities
{
    // A one-time code sent to confirm an email address or authorize a sensitive action.
    // Protects its own invariants: it cannot expire before it was created, and it always starts
    // with zero attempts and unused.
    public class VerificationCode
    {
        public VerificationCode(int playerId, VerificationPurpose purpose, string codeHash, DateTime expiresAt)
        {
            DateTime creationMoment = DateTime.UtcNow;
            ValidatePlayerId(playerId);
            ValidateCodeHash(codeHash);
            ValidateExpiry(expiresAt, creationMoment);

            PlayerId = playerId;
            Purpose = purpose;
            CodeHash = codeHash;
            ExpiresAt = expiresAt;
            CreatedAt = creationMoment;
            Attempts = 0;
            UsedAt = null;
        }

        // Parameterless constructor reserved for Entity Framework's materialization; not for
        // application code, which must use the validating constructor.
        protected VerificationCode()
        {
        }

        public int VerificationCodeId { get; private set; }
        public int PlayerId { get; private set; }
        public VerificationPurpose Purpose { get; private set; }
        // Only the hash is stored, never the plain code itself.
        public string CodeHash { get; private set; }
        public DateTime ExpiresAt { get; private set; }
        public DateTime? UsedAt { get; private set; }
        public int Attempts { get; private set; }
        public DateTime CreatedAt { get; private set; }

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
