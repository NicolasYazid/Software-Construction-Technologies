using System.Windows;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Error dialog (P22). Presents the data, network, session and game exceptions that the
    /// use cases send to it, each one with its own title, message and actions.
    /// </summary>
    public partial class GuiErrorDialog : GuiModalBase
    {
        private string _titleKey;
        private string _messageKey;

        public GuiErrorDialog(ErrorDialogKind kind)
        {
            InitializeComponent();
            ApplyKind(kind);
            RefreshFormattedText();
        }

        // The second action is retrying or returning to the match, depending on the error.
        public bool IsActionChosen { get; private set; }

        protected override void RefreshFormattedText()
        {
            if (lblTitle != null)
            {
                string title = Localization.GetText(_titleKey);
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
