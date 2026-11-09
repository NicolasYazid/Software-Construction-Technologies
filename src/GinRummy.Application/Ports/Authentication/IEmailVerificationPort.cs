namespace GinRummy.Application.Ports.Authentication
{
    /// <summary>
    /// Entry point of the verification of the email of a new account and of the request of a new code
    /// (CU-09). The network boundary calls it without knowing which use case answers.
    /// </summary>
    public interface IEmailVerificationPort
    {
        /// <summary>
        /// Verifies the email of an account by redeeming the code emailed to it.
        /// </summary>
        /// <param name="email">Email of the account, exactly as the client has it.</param>
        /// <param name="verificationCode">Code exactly as the player typed it.</param>
        void VerifyEmail(string email, string verificationCode);

        /// <summary>
        /// Emails a new code to an account whose email is not verified yet, replacing the pending one.
        /// </summary>
        /// <param name="email">Email of the account, exactly as the client has it.</param>
        void ResendVerificationCode(string email);
    }
}
