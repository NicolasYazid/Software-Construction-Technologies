using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GinRummy.Client.Controllers
{
    // Outcome of trying to sign in: either the signed-in player's username, or the localization
    // key of the message to show.
    public class LogInResult
    {
        private LogInResult(bool succeeded, string errorMessageKey, string username)
        {
            Succeeded = succeeded;
            ErrorMessageKey = errorMessageKey;
            Username = username;
        }

        public bool Succeeded { get; }
        public string ErrorMessageKey { get; }
        public string Username { get; }

        public static LogInResult Success(string username)
        {
            return new LogInResult(true, null, username);
        }

        public static LogInResult Failure(string errorMessageKey)
        {
            return new LogInResult(false, errorMessageKey, null);
        }
    }
}
