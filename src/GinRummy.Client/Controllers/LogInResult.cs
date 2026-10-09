namespace GinRummy.Client.Controllers
{
    public class LogInResult
    {
        private LogInResult(bool isSuccessful, string errorMessageKey, string username)
        {
            IsSuccessful = isSuccessful;
            ErrorMessageKey = errorMessageKey;
            Username = username;
        }

        public bool IsSuccessful { get; }
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
