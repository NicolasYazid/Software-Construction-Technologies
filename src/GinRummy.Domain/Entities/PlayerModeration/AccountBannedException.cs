using System;

namespace GinRummy.Domain.Entities.PlayerModeration
{
    public class AccountBannedException : GameCoreException
    {
        public AccountBannedException()
        {
        }

        public AccountBannedException(string message)
            : base(message)
        {
        }

        public AccountBannedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public AccountBannedException(string message, string reasonName, DateTime? expiresAt)
            : base(message)
        {
            ReasonName = reasonName;
            ExpiresAt = expiresAt;
        }

        public string ReasonName { get; private set; }

        public DateTime? ExpiresAt { get; private set; }
    }
}
