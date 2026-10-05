namespace GinRummy.Client.Views
{
    // The six destructive actions the confirmation dialog (P23) asks about. Each one fixes the
    // title, the warning and the label of the button that confirms it.
    public enum ConfirmDialogKind
    {
        // Leaving a match in progress, which counts as a defeat (CU-26).
        Forfeit,
        // Logging out with a match in progress (CU-03 FA-02).
        LogOut,
        // Removing a player from the friend list (CU-16).
        RemoveFriend,
        // Removing a social link from the profile (CU-32).
        SocialLinkRemove,
        // Replacing the link of a platform that already has one (CU-31 FA-03).
        SocialLinkReplace,
        // Turning off the two-step verification, which asks for the password (CU-05).
        TwoStepDisable
    }
}
