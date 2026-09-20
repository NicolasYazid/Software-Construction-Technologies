namespace GinRummy.Client.Views
{
    /// <summary>
    /// Reason why the two-step verification screen was opened. Each reason has its own
    /// instruction text in the dictionary.
    /// </summary>
    public enum TwoStepPurpose
    {
        /// <summary>
        /// The code is the second factor of a sign-in.
        /// </summary>
        LogIn,

        /// <summary>
        /// The code confirms turning two-step verification on.
        /// </summary>
        EnableTwoStep,

        /// <summary>
        /// The code confirms a password change.
        /// </summary>
        ChangePassword
    }
}
