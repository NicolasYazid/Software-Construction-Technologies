using GinRummy.Domain.Entities;

namespace GinRummy.Application.Ports
{
    /// <summary>
    /// Entry point of the confirmation of a sign-in with the code of the second factor (CU-02 FA-04). The
    /// network boundary calls it without knowing which use case answers.
    /// </summary>
    public interface ITwoFactorLogInPort
    {
        /// <summary>
        /// Completes a sign-in whose password was accepted by redeeming the code emailed to the account.
        /// </summary>
        /// <param name="email">Email of the account, exactly as the player typed it to sign in.</param>
        /// <param name="verificationCode">Code exactly as the player typed it.</param>
        /// <param name="keepSignedIn">Whether the player asked to stay signed in.</param>
        /// <returns>The opened session.</returns>
        LogInResult VerifyTwoFactorLogIn(string email, string verificationCode, bool keepSignedIn);
    }
}
