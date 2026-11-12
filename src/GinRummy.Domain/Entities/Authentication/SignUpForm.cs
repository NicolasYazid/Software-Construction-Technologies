namespace GinRummy.Domain.Entities.Authentication
{
    public class SignUpForm
    {
        public SignUpForm(string email, string username, string password)
        {
            Email = email;
            Username = username;
            Password = password;
        }

        public string Email { get; private set; }

        public string Username { get; private set; }

        public string Password { get; private set; }
    }
}
