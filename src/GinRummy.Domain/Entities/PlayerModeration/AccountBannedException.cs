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
    }
}
