using GinRummy.Client.Models;

namespace GinRummy.Client.Views
{
    // The sign-in screen and the two-step screen both end on these screens when a ban restricts the account.
    public static class AccountRestrictionScreenCommon
    {
        // A temporary ban shows its time left (CU-02 EX-04) and a permanent one the link to the terms (CU-02 EX-05).
        public static GuiModalBase CreateScreen(AccountRestriction restriction)
        {
            GuiModalBase screen;
            if (restriction.IsPermanent)
            {
                screen = new GuiAccountBanned(restriction.ReasonName);
            }
            else
            {
                screen = new GuiAccountSuspended(restriction.ReasonName, restriction.RemainingTime.Value);
            }

            return screen;
        }
    }
}
