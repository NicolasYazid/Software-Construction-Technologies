using System.Windows;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Confirmation dialog (P23). Asks before the destructive actions of CU-03, CU-05, CU-16,
    /// CU-26, CU-31 and CU-32, each one with its own title, warning and confirming button.
    /// </summary>
    public partial class GuiConfirmDialog : GuiModalBase
    {
        private const string MessageStyleKey = "StyDialogMessage";
        private const string WarningStyleKey = "StyDialogWarning";

        private string _titleKey;
        private string _messageKey;
        private string _confirmKey;

        public GuiConfirmDialog(ConfirmDialogKind kind)
        {
            InitializeComponent();
            ApplyKind(kind);
            RefreshFormattedText();
        }

        public bool IsConfirmed { get; private set; }

        public string EnteredPassword
        {
            get { return pwdPassword.Password; }
        }

        protected override void RefreshFormattedText()
        {
            if (lblTitle != null)
            {
                string title = Localization.GetText(_titleKey);
                lblTitle.Text = title;
                lblMessage.Text = Localization.GetText(_messageKey);
                btnConfirm.Content = Localization.GetText(_confirmKey).ToUpper(Localization.CurrentCulture);
            }
        }

        private void ApplyKind(ConfirmDialogKind kind)
        {
            switch (kind)
            {
                case ConfirmDialogKind.Forfeit:
                    SetTextKeys("ConfirmDialog_LblTitleForfeit", "GameTable_ForfeitWarning", "GameTable_BtnForfeitConfirm");
                    break;
                case ConfirmDialogKind.LogOut:
                    SetTextKeys("ConfirmDialog_LblTitleLogOut", "ProfilePanel_LogOutForfeitWarning", "Shared_BtnLogOut");
                    break;
                case ConfirmDialogKind.SocialLinkRemove:
                    SetTextKeys(
                        "ConfirmDialog_LblTitleSocialLinkRemove",
                        "EditProfile_SocialLinkRemoveConfirm",
                        "EditProfile_BtnSocialLinkRemoveConfirm");
                    break;
                case ConfirmDialogKind.SocialLinkReplace:
                    SetTextKeys(
                        "ConfirmDialog_LblTitleSocialLinkReplace",
                        "EditProfile_SocialLinkReplaceConfirm",
                        "EditProfile_BtnSocialLinkReplaceConfirm");
                    break;
                case ConfirmDialogKind.TwoStepDisable:
                    ApplyTwoStepDisableKind();
                    break;
                default:
                    SetTextKeys("ConfirmDialog_LblTitleRemoveFriend", "FriendsList_RemoveConfirm", "FriendsList_BtnRemoveConfirm");
                    break;
            }

            lblMessage.Style = (Style)FindResource(ResolveMessageStyleKey(kind));
        }

        private void ApplyTwoStepDisableKind()
        {
            SetTextKeys(
                "ConfirmDialog_LblTitleTwoStepDisable",
                "ProfilePanel_TwoStepDisableWarning",
                "ProfilePanel_BtnTwoStepDisableConfirm");
            lblPassword.Visibility = Visibility.Visible;
        }

        private void SetTextKeys(string titleKey, string messageKey, string confirmKey)
        {
            _titleKey = titleKey;
            _messageKey = messageKey;
            _confirmKey = confirmKey;
        }

        // The prototype paints red the warnings of an action with consequences for the account or the match.
        // The plain questions about the profile stay in grey.
        private static string ResolveMessageStyleKey(ConfirmDialogKind kind)
        {
            string messageStyleKey = MessageStyleKey;
            switch (kind)
            {
                case ConfirmDialogKind.Forfeit:
                case ConfirmDialogKind.LogOut:
                case ConfirmDialogKind.TwoStepDisable:
                    messageStyleKey = WarningStyleKey;
                    break;
                default:
                    break;
            }

            return messageStyleKey;
        }

        private void OnConfirmClick(object sender, RoutedEventArgs e)
        {
            IsConfirmed = true;
            Close();
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
