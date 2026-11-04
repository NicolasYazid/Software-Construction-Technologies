using GinRummy.Client.Models;

namespace GinRummy.Client.Views
{
    // The sign-in screen and the two-step screen both end on these screens when a ban restricts the account.
    public static class AccountRestrictionScreenCommon
    {
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
