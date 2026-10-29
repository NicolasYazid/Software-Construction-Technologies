using GinRummy.Client.Mvvm;

namespace GinRummy.Client.ViewModels
{
    public class ConfirmDialogViewModel : ViewModelBase
    {
        private const string ForfeitTitleKey = "ConfirmDialog_LblTitleForfeit";
        private const string ForfeitMessageKey = "GameTable_ForfeitWarning";
        private const string ForfeitConfirmKey = "GameTable_BtnForfeitConfirm";
        private const string LogOutTitleKey = "ConfirmDialog_LblTitleLogOut";
        private const string LogOutMessageKey = "ProfilePanel_LogOutForfeitWarning";
        private const string LogOutConfirmKey = "Shared_BtnLogOut";
        private const string SocialLinkRemoveTitleKey = "ConfirmDialog_LblTitleSocialLinkRemove";
        private const string SocialLinkRemoveMessageKey = "EditProfile_SocialLinkRemoveConfirm";
        private const string SocialLinkRemoveConfirmKey = "EditProfile_BtnSocialLinkRemoveConfirm";
        private const string SocialLinkReplaceTitleKey = "ConfirmDialog_LblTitleSocialLinkReplace";
        private const string SocialLinkReplaceMessageKey = "EditProfile_SocialLinkReplaceConfirm";
        private const string SocialLinkReplaceConfirmKey = "EditProfile_BtnSocialLinkReplaceConfirm";
        private const string TwoStepDisableTitleKey = "ConfirmDialog_LblTitleTwoStepDisable";
        private const string TwoStepDisableMessageKey = "ProfilePanel_TwoStepDisableWarning";
        private const string TwoStepDisableConfirmKey = "ProfilePanel_BtnTwoStepDisableConfirm";
        private const string RemoveFriendTitleKey = "ConfirmDialog_LblTitleRemoveFriend";
        private const string RemoveFriendMessageKey = "FriendsList_RemoveConfirm";
        private const string RemoveFriendConfirmKey = "FriendsList_BtnRemoveConfirm";

        public ConfirmDialogViewModel(ConfirmDialogKind kind)
        {
            ApplyKind(kind);
            Password = string.Empty;
        }

        public string TitleKey { get; private set; }
        public string MessageKey { get; private set; }
        public string ConfirmKey { get; private set; }
        public bool IsWarning { get; private set; }
        public bool IsPasswordRequired { get; private set; }
        public string Password { get; set; }

        private void ApplyKind(ConfirmDialogKind kind)
        {
            // The prototype paints red the warnings of an action with consequences for the account or the match.
            // The plain questions about the profile stay in grey.
            switch (kind)
            {
                case ConfirmDialogKind.Forfeit:
                    SetTextKeys(ForfeitTitleKey, ForfeitMessageKey, ForfeitConfirmKey);
                    IsWarning = true;
                    break;
                case ConfirmDialogKind.LogOut:
                    SetTextKeys(LogOutTitleKey, LogOutMessageKey, LogOutConfirmKey);
                    IsWarning = true;
                    break;
                case ConfirmDialogKind.SocialLinkRemove:
                    SetTextKeys(SocialLinkRemoveTitleKey, SocialLinkRemoveMessageKey, SocialLinkRemoveConfirmKey);
                    break;
                case ConfirmDialogKind.SocialLinkReplace:
                    SetTextKeys(SocialLinkReplaceTitleKey, SocialLinkReplaceMessageKey, SocialLinkReplaceConfirmKey);
                    break;
                case ConfirmDialogKind.TwoStepDisable:
                    ApplyTwoStepDisableKind();
                    break;
                default:
                    SetTextKeys(RemoveFriendTitleKey, RemoveFriendMessageKey, RemoveFriendConfirmKey);
                    break;
            }
        }

        // CU-05 makes the player prove again that the account is theirs before the second factor is turned off.
        private void ApplyTwoStepDisableKind()
        {
            SetTextKeys(TwoStepDisableTitleKey, TwoStepDisableMessageKey, TwoStepDisableConfirmKey);
            IsWarning = true;
            IsPasswordRequired = true;
        }

        private void SetTextKeys(string titleKey, string messageKey, string confirmKey)
        {
            TitleKey = titleKey;
            MessageKey = messageKey;
            ConfirmKey = confirmKey;
        }
    }
}
