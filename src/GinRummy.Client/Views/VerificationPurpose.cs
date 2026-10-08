namespace GinRummy.Client.Views
{
    // Reason why the verification code screen was opened.
    // Each reason has its own title in the dictionary, because a single key cannot hold three different values.
    public enum VerificationPurpose
    {
        AccountSignUp,
        PasswordRecovery,
        EmailChange
    }
}
