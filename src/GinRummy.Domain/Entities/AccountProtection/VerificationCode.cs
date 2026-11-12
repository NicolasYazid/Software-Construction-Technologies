using System;

namespace GinRummy.Domain.Entities.AccountProtection
{
    public class VerificationCode
    {
        public const int CodeHashLength = 64;

        public VerificationCode(
            int playerAccountId,
            VerificationPurpose purpose,
            string codeHash,
            DateTime createdAt,
            DateTime expiresAt)
        {
            ValidatePlayerAccountId(playerAccountId);
            ValidateCodeHash(codeHash);
            ValidateExpiry(expiresAt, createdAt);

            PlayerAccountId = playerAccountId;
            Purpose = purpose;
            CodeHash = codeHash;
            ExpiresAt = expiresAt;
            CreatedAt = createdAt;
            Attempts = 0;
            UsedAt = null;
        }

        protected VerificationCode()
        {
        }

        public int VerificationCodeId { get; private set; }

        public int PlayerAccountId { get; private set; }

        public VerificationPurpose Purpose { get; private set; }

        public string CodeHash { get; private set; }

        public DateTime ExpiresAt { get; private set; }

        public DateTime? UsedAt { get; private set; }

        public int Attempts { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public void RegisterFailedAttempt()
        {
            Attempts++;
        }

        public void MarkAsUsed(DateTime usedAt)
        {
            if (UsedAt.HasValue)
            {
                throw new InvalidOperationException("The verification code was already used.");
            }

            UsedAt = usedAt;
        }

        private static void ValidatePlayerAccountId(int playerAccountId)
        {
            if (playerAccountId <= 0)
            {
                throw new ArgumentException("Player account id must be positive.", nameof(playerAccountId));
            }
        }

        private static void ValidateCodeHash(string codeHash)
        {
            if (string.IsNullOrWhiteSpace(codeHash))
            {
                throw new ArgumentException("Code hash is required.", nameof(codeHash));
            }
        }

        private static void ValidateExpiry(DateTime expiresAt, DateTime creationMoment)
        {
            if (expiresAt <= creationMoment)
            {
                throw new ArgumentException("Expiry must be after creation.", nameof(expiresAt));
            }
        }
    }
}
