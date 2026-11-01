using System;

namespace GinRummy.Domain.Entities
{
    /// <summary>
    /// Outcome of a sign-in whose credentials were accepted: either the session that was opened, or the
    /// notice that the account still has to confirm the code of its second factor (CU-02 FA-04).
    /// </summary>
    public class LogInResult
    {
        private LogInResult(PlayerAccount account, PlayerSession openedSession)
        {
            Account = account ?? throw new ArgumentNullException(nameof(account));
            OpenedSession = openedSession;
        }

        /// <summary>
        /// Gets the account that signed in, with its player loaded.
        /// </summary>
        public PlayerAccount Account { get; private set; }

        /// <summary>
        /// Gets the session that was opened, or null while the second factor is pending.
        /// </summary>
        public PlayerSession OpenedSession { get; private set; }

        /// <summary>
        /// Gets whether the account must still confirm the code of its second factor.
        /// </summary>
        public bool IsTwoFactorRequired
        {
            get { return OpenedSession == null; }
        }

        /// <summary>
        /// Builds the outcome of a sign-in that opened a session.
        /// </summary>
        /// <param name="account">Account that signed in.</param>
        /// <param name="openedSession">Session that was opened.</param>
        /// <returns>The outcome with its session.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the account or the session is null.</exception>
        public static LogInResult ForOpenedSession(PlayerAccount account, PlayerSession openedSession)
        {
            if (openedSession == null)
            {
                throw new ArgumentNullException(nameof(openedSession));
            }

            return new LogInResult(account, openedSession);
        }

        /// <summary>
        /// Builds the outcome of a sign-in that is waiting for the code of the second factor.
        /// </summary>
        /// <param name="account">Account that signed in.</param>
        /// <returns>The outcome without a session.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the account is null.</exception>
        public static LogInResult ForPendingTwoFactor(PlayerAccount account)
        {
            return new LogInResult(account, null);
        }
    }
}
