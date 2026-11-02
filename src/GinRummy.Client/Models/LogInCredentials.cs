namespace GinRummy.Client.Models
{
    // The two-step screen signs in again with the same credentials when the player asks for a new code.
    // They therefore travel together from the sign-in screen and live only in memory, never on disk.
    public sealed class LogInCredentials
    {
        public LogInCredentials(string email, string password, bool isKeepSignedIn)
        {
            Email = email;
            Password = password;
            IsKeepSignedIn = isKeepSignedIn;
        }

        public string Email { get; private set; }
        public string Password { get; private set; }
        public bool IsKeepSignedIn { get; private set; }
    }
}
