namespace GinRummy.Client.Views
{
    /// <summary>
    /// Reason why the verification code screen was opened. Each reason has its own title
    /// in the dictionary, because a single key cannot hold three different values.
    /// </summary>
    public enum VerificationPurpose
    {
        /// <summary>
        /// The code confirms the email address of a new account.
        /// </summary>
        AccountSignUp,

        /// <summary>
        /// The code allows the player to set a new password.
        /// </summary>
        PasswordRecovery,

        /// <summary>
        /// The code confirms a new email address.
        /// </summary>
        EmailChange
    }
}
