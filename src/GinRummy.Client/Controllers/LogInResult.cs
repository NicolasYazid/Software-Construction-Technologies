using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GinRummy.Client.Controllers
{
    /// <summary>
    /// Outcome of trying to sign in: either the signed-in player's username, or the
    /// localization key of the message to show.
    /// </summary>
    public class LogInResult
    {
        private LogInResult(bool succeeded, string errorMessageKey, string username)
        {
            Succeeded = succeeded;
            ErrorMessageKey = errorMessageKey;
            Username = username;
        }

        /// <summary>
        /// Gets whether the sign-in succeeded.
        /// </summary>
        public bool Succeeded { get; }

        /// <summary>
        /// Gets the localization key of the error to show, or null on success.
        /// </summary>
        public string ErrorMessageKey { get; }

        /// <summary>
        /// Gets the username of the signed-in player, or null on failure.
        /// </summary>
        public string Username { get; }

        /// <summary>
        /// Builds a successful result carrying the player's username.
        /// </summary>
        /// <param name="username">Username of the signed-in player.</param>
        /// <returns>A successful result.</returns>
        public static LogInResult Success(string username)
        {
            return new LogInResult(true, null, username);
        }

        /// <summary>
        /// Builds a failed result carrying which message to show.
        /// </summary>
        /// <param name="errorMessageKey">Localization key of the message to show.</param>
        /// <returns>A failed result.</returns>
        public static LogInResult Failure(string errorMessageKey)
        {
            return new LogInResult(false, errorMessageKey, null);
        }
    }
}
