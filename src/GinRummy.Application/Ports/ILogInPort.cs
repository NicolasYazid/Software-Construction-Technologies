using GinRummy.Domain.Entities;

namespace GinRummy.Application.Ports
{
    /// <summary>
    /// Entry point of the sign-in with email and password (CU-02). The network boundary calls it without
    /// knowing which use case answers.
    /// </summary>
    public interface ILogInPort
    {
        /// <summary>
        /// Signs a player in with the email and the password of the account.
        /// </summary>
        /// <param name="email">Email exactly as the player typed it.</param>
        /// <param name="password">Password exactly as the player typed it.</param>
        /// <param name="keepSignedIn">Whether the player asked to stay signed in.</param>
        /// <returns>The opened session, or the notice that the second factor is pending.</returns>
        LogInResult LogIn(string email, string password, bool keepSignedIn);
    }
}
