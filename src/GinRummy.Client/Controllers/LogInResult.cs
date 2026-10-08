namespace GinRummy.Client.Controllers
{
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
