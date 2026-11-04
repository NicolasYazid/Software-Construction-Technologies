namespace GinRummy.Client.ViewModels
{
    public enum ConfirmDialogKind
    {
        /// <summary>
        /// Leaving a match in progress, which counts as a defeat (CU-26).
        /// </summary>
        Forfeit,

        /// <summary>
        /// Logging out with a match in progress (CU-03 FA-02).
        /// </summary>
        LogOut,

        /// <summary>
        /// Removing a player from the friend list (CU-16).
        /// </summary>
        RemoveFriend,

        /// <summary>
        /// Removing a social link from the profile (CU-32).
        /// </summary>
        SocialLinkRemove,

        /// <summary>
        /// Replacing the link of a platform that already has one (CU-31 FA-03).
        /// </summary>
        SocialLinkReplace,

        /// <summary>
        /// Turning off the two-step verification, which asks for the password (CU-05).
        /// </summary>
        TwoStepDisable
    }
}
