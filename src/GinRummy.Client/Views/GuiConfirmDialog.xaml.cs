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

        /// <summary>
        /// Builds the dialog for one of the known actions.
        /// </summary>
        /// <param name="kind">Action to confirm.</param>
        public GuiConfirmDialog(ConfirmDialogKind kind)
        {
            InitializeComponent();
            ApplyKind(kind);
            RefreshFormattedText();
        }

        /// <summary>
        /// Gets whether the player confirmed the action.
        /// </summary>
        public bool IsConfirmed { get; private set; }

        /// <summary>
        /// Gets the password typed in the dialog that turns off the two-step verification.
        /// </summary>
        public string EnteredPassword
        {
            get { return pwdPassword.Password; }
        }

        /// <summary>
        /// Rebuilds the title, the warning and the confirming button, which come from a
        /// different key for each action and therefore cannot be bound in XAML to a single one.
        /// </summary>
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

        // The prototype paints red the warnings of an action with consequences for the account
        // or the match, and leaves in grey the plain questions about the profile.
        private void ApplyKind(ConfirmDialogKind kind)
        {
            string messageStyleKey = MessageStyleKey;
            switch (kind)
            {
                case ConfirmDialogKind.Forfeit:
                    _titleKey = "ConfirmDialog_LblTitleForfeit";
                    _messageKey = "GameTable_ForfeitWarning";
                    _confirmKey = "GameTable_BtnForfeitConfirm";
                    messageStyleKey = WarningStyleKey;
                    break;
                case ConfirmDialogKind.LogOut:
                    _titleKey = "ConfirmDialog_LblTitleLogOut";
                    _messageKey = "ProfilePanel_LogOutForfeitWarning";
                    _confirmKey = "Shared_BtnLogOut";
                    messageStyleKey = WarningStyleKey;
                    break;
                case ConfirmDialogKind.SocialLinkRemove:
                    _titleKey = "ConfirmDialog_LblTitleSocialLinkRemove";
                    _messageKey = "EditProfile_SocialLinkRemoveConfirm";
                    _confirmKey = "EditProfile_BtnSocialLinkRemoveConfirm";
                    break;
                case ConfirmDialogKind.SocialLinkReplace:
                    _titleKey = "ConfirmDialog_LblTitleSocialLinkReplace";
                    _messageKey = "EditProfile_SocialLinkReplaceConfirm";
                    _confirmKey = "EditProfile_BtnSocialLinkReplaceConfirm";
                    break;
                case ConfirmDialogKind.TwoStepDisable:
                    _titleKey = "ConfirmDialog_LblTitleTwoStepDisable";
                    _messageKey = "ProfilePanel_TwoStepDisableWarning";
                    _confirmKey = "ProfilePanel_BtnTwoStepDisableConfirm";
                    messageStyleKey = WarningStyleKey;
                    lblPassword.Visibility = Visibility.Visible;
                    break;
                default:
                    _titleKey = "ConfirmDialog_LblTitleRemoveFriend";
                    _messageKey = "FriendsList_RemoveConfirm";
                    _confirmKey = "FriendsList_BtnRemoveConfirm";
                    break;
            }

            lblMessage.Style = (Style)FindResource(messageStyleKey);
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
