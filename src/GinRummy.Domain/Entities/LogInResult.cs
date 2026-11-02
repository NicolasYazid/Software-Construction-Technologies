using System;

namespace GinRummy.Domain.Entities
{
    /// <summary>
    /// Outcome of a sign-in whose credentials were accepted: either the access token that was granted, or
    /// the notice that the account still has to confirm the code of its second factor (CU-02 FA-04).
    /// </summary>
    public class LogInResult
    {
        private LogInResult(PlayerAccount account, AccessToken accessToken)
        {
            Account = account ?? throw new ArgumentNullException(nameof(account));
            AccessToken = accessToken;
        }

        /// <summary>
        /// Gets the account that signed in, with its player loaded.
        /// </summary>
        public PlayerAccount Account { get; private set; }

        /// <summary>
        /// Gets the access token that was granted, or null while the second factor is pending.
        /// </summary>
        public AccessToken AccessToken { get; private set; }

        /// <summary>
        /// Gets whether the account must still confirm the code of its second factor.
        /// </summary>
        public bool IsTwoFactorRequired
        {
            get { return AccessToken == null; }
        }

        /// <summary>
        /// Builds the outcome of a sign-in that granted an access token.
        /// </summary>
        /// <param name="account">Account that signed in.</param>
        /// <param name="accessToken">Access token that was granted.</param>
        /// <returns>The outcome with its token.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the account or the token is null.</exception>
        public static LogInResult ForSignedIn(PlayerAccount account, AccessToken accessToken)
        {
            if (accessToken == null)
            {
                throw new ArgumentNullException(nameof(accessToken));
            }

            return new LogInResult(account, accessToken);
        }

        /// <summary>
        /// Builds the outcome of a sign-in that is waiting for the code of the second factor.
        /// </summary>
        /// <param name="account">Account that signed in.</param>
        /// <returns>The outcome without a token.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the account is null.</exception>
        public static LogInResult ForPendingTwoFactor(PlayerAccount account)
        {
            return new LogInResult(account, null);
        }
    }
}
