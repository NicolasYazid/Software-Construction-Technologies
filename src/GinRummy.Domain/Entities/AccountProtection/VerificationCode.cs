using System;

using GinRummy.Domain.Shared;

namespace GinRummy.Domain.Entities.AccountProtection
{
    public class VerificationCode
    {
        public const int CodeHashLength = 64;

        public VerificationCode(VerificationRequest request, string codeHash, ValidityPeriod validity)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (validity == null)
            {
                throw new ArgumentNullException(nameof(validity));
            }

            ValidatePlayerAccountId(request.PlayerAccountId);
            ValidateCodeHash(codeHash);
            ValidateExpiry(validity.ExpiresAt, validity.CreatedAt);

            PlayerAccountId = request.PlayerAccountId;
            Purpose = request.Purpose;
            CodeHash = codeHash;
            ExpiresAt = validity.ExpiresAt;
            CreatedAt = validity.CreatedAt;
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
