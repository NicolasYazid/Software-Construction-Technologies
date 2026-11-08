namespace GinRummy.Domain.Entities.Authentication
{
    /// <summary>
    /// What a person typed in the sign-up screen to create an account (CU-01 step 3). Nothing in it is checked
    /// yet; the registrar checks it before creating anything, so every broken rule ends as its own exception.
    /// </summary>
    public class SignUpForm
    {
        /// <summary>
        /// Builds the form with the three fields the person typed.
        /// </summary>
        /// <param name="email">Email exactly as it was typed.</param>
        /// <param name="username">Username exactly as it was typed.</param>
        /// <param name="password">Password exactly as it was typed.</param>
        public SignUpForm(string email, string username, string password)
        {
            Email = email;
            Username = username;
            Password = password;
        }

        /// <summary>
        /// Gets the email exactly as it was typed.
        /// </summary>
        public string Email { get; private set; }

        /// <summary>
        /// Gets the username exactly as it was typed.
        /// </summary>
        public string Username { get; private set; }

        /// <summary>
        /// Gets the password exactly as it was typed.
        /// </summary>
        public string Password { get; private set; }
    }
}
