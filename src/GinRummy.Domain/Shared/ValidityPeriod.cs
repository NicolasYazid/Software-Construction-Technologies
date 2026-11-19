using System;

namespace GinRummy.Domain.Shared
{
    public class ValidityPeriod
    {
        public ValidityPeriod(DateTime createdAt, DateTime expiresAt)
        {
            CreatedAt = createdAt;
            ExpiresAt = expiresAt;
        }

        public DateTime CreatedAt { get; private set; }

        public DateTime ExpiresAt { get; private set; }
    }
}
