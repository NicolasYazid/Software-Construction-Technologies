using System;

namespace GinRummy.Domain.Entities.PlayerModeration
{
    public class Ban
    {
        public Ban(int playerAccountId, int reasonBanId, BanTerm term)
        {
            if (term == null)
            {
                throw new ArgumentNullException(nameof(term));
            }

            ValidateId(playerAccountId, nameof(playerAccountId));
            ValidateId(reasonBanId, nameof(reasonBanId));
            ValidateExpiry(term.IssuedAt, term.ExpiresAt);

            PlayerAccountId = playerAccountId;
            ReasonBanId = reasonBanId;
            IssuedAt = term.IssuedAt;
            ExpiresAt = term.ExpiresAt;
        }

        protected Ban()
        {
        }

        public int BanId { get; private set; }

        public int PlayerAccountId { get; private set; }

        public int ReasonBanId { get; private set; }

        public DateTime IssuedAt { get; private set; }

        public DateTime? ExpiresAt { get; private set; }

        public virtual ReasonBan ReasonBan { get; private set; }

        public bool IsPermanent
        {
            get { return !ExpiresAt.HasValue; }
        }

        public bool IsActiveAt(DateTime moment)
        {
            bool isActive = IsPermanent || (ExpiresAt.Value > moment);

            return isActive;
        }

        private static void ValidateId(int id, string parameterName)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Id must be positive.", parameterName);
            }
        }

        private static void ValidateExpiry(DateTime issuedAt, DateTime? expiresAt)
        {
            if (expiresAt.HasValue && (expiresAt.Value <= issuedAt))
            {
                throw new ArgumentException("Expiry must be after the issue moment.", nameof(expiresAt));
            }
        }
    }
}
