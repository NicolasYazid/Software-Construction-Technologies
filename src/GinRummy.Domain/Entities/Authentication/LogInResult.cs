using System;

namespace GinRummy.Domain.Entities.Authentication
{
    public class LogInResult
    {
        private LogInResult(PlayerAccount account, AccessToken accessToken)
        {
            Account = account ?? throw new ArgumentNullException(nameof(account));
            AccessToken = accessToken;
        }

        public PlayerAccount Account { get; private set; }

        public AccessToken AccessToken { get; private set; }

        public bool IsTwoFactorRequired
        {
            get { return AccessToken == null; }
        }

        public static LogInResult ForSignedIn(PlayerAccount account, AccessToken accessToken)
        {
            if (accessToken == null)
            {
                throw new ArgumentNullException(nameof(accessToken));
            }

            return new LogInResult(account, accessToken);
        }

        public static LogInResult ForPendingTwoFactor(PlayerAccount account)
        {
            return new LogInResult(account, null);
        }
    }
}
