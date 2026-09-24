using System.Windows;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Error dialog (P22). Presents the data, network, session and game exceptions that the
    /// use cases send to it, each one with its own title, message and actions.
    /// </summary>
    public partial class GuiErrorDialog : GuiWindowBase
    {
        private string _titleKey;
        private string _messageKey;

        /// <summary>
        /// Builds the dialog for one of the known errors.
        /// </summary>
        /// <param name="kind">Error to present.</param>
        public GuiErrorDialog(ErrorDialogKind kind)
        {
            InitializeComponent();
            ApplyKind(kind);
            RefreshFormattedText();
        }

        /// <summary>
        /// Gets whether the player chose the second action (retry, or return to the match)
        /// instead of closing the dialog.
        /// </summary>
        public bool IsActionChosen { get; private set; }

        /// <summary>
        /// Rebuilds the title and the message, which come from a different key for each error
        /// and therefore cannot be bound in XAML to a single one.
        /// </summary>
        protected override void RefreshFormattedText()
        {
            if (lblTitle != null)
            {
                string title = Localization.GetText(_titleKey);
                Title = title;
                lblTitle.Text = title;
                lblMessage.Text = Localization.GetText(_messageKey);
            }
        }

        private void ApplyKind(ErrorDialogKind kind)
        {
            switch (kind)
            {
                case ErrorDialogKind.ConnectionRefused:
                    _titleKey = "ErrorDialog_LblTitleConnectionRefused";
                    _messageKey = "Error_NetConnectionRefused";
                    btnRetry.Visibility = Visibility.Visible;
                    break;
                case ErrorDialogKind.Unexpected:
                    _titleKey = "ErrorDialog_LblTitleUnexpected";
                    _messageKey = "Error_SysUnexpected";
                    break;
                case ErrorDialogKind.ConnectionLost:
                    _titleKey = "ErrorDialog_LblTitleConnectionLost";
                    _messageKey = "Error_NetConnectionLost";
                    break;
                case ErrorDialogKind.SessionExpired:
                    _titleKey = "ErrorDialog_LblTitleSessionExpired";
                    _messageKey = "Error_AuthSessionExpired";
                    break;
                case ErrorDialogKind.LogOutNotRecorded:
                    _titleKey = "ErrorDialog_LblTitleLogOutNotRecorded";
                    _messageKey = "Error_SysLogOutNotRecorded";
                    break;
                case ErrorDialogKind.SessionClosedElsewhere:
                    _titleKey = "ErrorDialog_LblTitleSessionClosedElsewhere";
                    _messageKey = "LogIn_SessionClosedElsewhere";
                    break;
                case ErrorDialogKind.PlayerBusy:
                    _titleKey = "ErrorDialog_LblTitlePlayerBusy";
                    _messageKey = "Error_GamePlayerBusy";
                    btnReturnToMatch.Visibility = Visibility.Visible;
                    break;
                case ErrorDialogKind.DataNotFound:
                    _titleKey = "ErrorDialog_LblTitleDataNotFound";
                    _messageKey = "Error_DataNotFound";
                    break;
                default:
                    _titleKey = "ErrorDialog_LblTitleServiceUnavailable";
                    _messageKey = "Error_SysServiceUnavailable";
                    break;
            }
        }

        private void OnActionClick(object sender, RoutedEventArgs e)
        {
            IsActionChosen = true;
            Close();
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
