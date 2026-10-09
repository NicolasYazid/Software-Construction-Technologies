using System;

using GinRummy.Domain.Dtos;

namespace GinRummy.Domain.Entities
{
    public class VerificationCode
    {
        public VerificationCode(NewVerificationCodeDto newCode)
        {
            DateTime creationMoment = DateTime.UtcNow;
            ValidateNewCode(newCode);
            ValidatePlayerId(newCode.PlayerId);
            ValidateCodeHash(newCode.CodeHash);
            ValidateExpiry(newCode.ExpiresAt, creationMoment);

            PlayerId = newCode.PlayerId;
            Purpose = newCode.Purpose;
            CodeHash = newCode.CodeHash;
            ExpiresAt = newCode.ExpiresAt;
            CreatedAt = creationMoment;
            Attempts = 0;
            UsedAt = null;
        }

        // Entity Framework needs a parameterless constructor to materialize rows.
        // It is protected so that application code cannot skip the validating constructor.
        protected VerificationCode()
        {
        }

        public int VerificationCodeId { get; private set; }
        public int PlayerId { get; private set; }
        public VerificationPurpose Purpose { get; private set; }
        public string CodeHash { get; private set; }
        public DateTime ExpiresAt { get; private set; }
        public DateTime? UsedAt { get; private set; }
        public int Attempts { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private static void ValidateNewCode(NewVerificationCodeDto newCode)
        {
            if (newCode == null)
            {
                throw new ArgumentNullException(nameof(newCode));
            }
        }

        private static void ValidatePlayerId(int playerId)
        {
            if (playerId <= 0)
            {
                throw new ArgumentException("Player id must be positive.", nameof(playerId));
            }
        }

        private static void ValidateCodeHash(string codeHash)
        {
            if (string.IsNullOrWhiteSpace(codeHash))
            {
                throw new ArgumentException("Code hash is required.", nameof(codeHash));
            }
        }

        // A code that expires at or before the moment it was created would be born already invalid.
        private static void ValidateExpiry(DateTime expiresAt, DateTime creationMoment)
        {
            if (expiresAt <= creationMoment)
            {
                throw new ArgumentException("Expiry must be after creation.", nameof(expiresAt));
            }
        }
    }
}
