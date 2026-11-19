using System;

namespace GinRummy.Domain.Entities.PlayerModeration
{
    public class BanTerm
    {
        public BanTerm(DateTime issuedAt, DateTime? expiresAt)
        {
            IssuedAt = issuedAt;
            ExpiresAt = expiresAt;
        }

        public DateTime IssuedAt { get; private set; }

        public DateTime? ExpiresAt { get; private set; }
    }
}
