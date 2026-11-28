using System;

namespace GinRummy.Domain.Entities.Authentication
{
    public class LogInResult
    {
        private LogInResult(LogInStatus status, PlayerAccount account, AccessToken accessToken)
        {
            Status = status;
            Account = account;
            AccessToken = accessToken;
        }

        public LogInStatus Status { get; private set; }
        public PlayerAccount Account { get; private set; }
        public AccessToken AccessToken { get; private set; }
        public string SanctionReasonName { get; private set; }
        public DateTime? SanctionExpiresAt { get; private set; }

        public bool IsTwoFactorRequired
        {
            get { return (Status == LogInStatus.Accepted) && (AccessToken == null) ? true : false; }
        }

        public static LogInResult ForSignedIn(PlayerAccount account, AccessToken accessToken)
        {
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            if (accessToken == null)
            {
                throw new ArgumentNullException(nameof(accessToken));
            }

            return new LogInResult(LogInStatus.Accepted, account, accessToken);
        }

        public static LogInResult ForPendingTwoFactor(PlayerAccount account)
        {
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            return new LogInResult(LogInStatus.Accepted, account, null);
        }

        public static LogInResult ForRejected(LogInStatus status)
        {
            return new LogInResult(status, null, null);
        }

        public static LogInResult ForBanned(string reasonName, DateTime? expiresAt)
        {
            LogInResult result = new LogInResult(LogInStatus.AccountBanned, null, null)
            {
                SanctionReasonName = reasonName,
                SanctionExpiresAt = expiresAt
            };

            return result;
        }
    }
}
