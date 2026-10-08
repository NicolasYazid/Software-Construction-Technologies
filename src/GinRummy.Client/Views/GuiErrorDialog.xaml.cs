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
                    ApplyConnectionRefusedKind();
                    break;
                case ErrorDialogKind.Unexpected:
                    SetTextKeys("ErrorDialog_LblTitleUnexpected", "Error_SysUnexpected");
                    break;
                case ErrorDialogKind.ConnectionLost:
                    SetTextKeys("ErrorDialog_LblTitleConnectionLost", "Error_NetConnectionLost");
                    break;
                case ErrorDialogKind.SessionExpired:
                    SetTextKeys("ErrorDialog_LblTitleSessionExpired", "Error_AuthSessionExpired");
                    break;
                case ErrorDialogKind.LogOutNotRecorded:
                    SetTextKeys("ErrorDialog_LblTitleLogOutNotRecorded", "Error_SysLogOutNotRecorded");
                    break;
                case ErrorDialogKind.SessionClosedElsewhere:
                    SetTextKeys("ErrorDialog_LblTitleSessionClosedElsewhere", "LogIn_SessionClosedElsewhere");
                    break;
                case ErrorDialogKind.PlayerBusy:
                    ApplyPlayerBusyKind();
                    break;
                case ErrorDialogKind.DataNotFound:
                    SetTextKeys("ErrorDialog_LblTitleDataNotFound", "Error_DataNotFound");
                    break;
                default:
                    SetTextKeys("ErrorDialog_LblTitleServiceUnavailable", "Error_SysServiceUnavailable");
                    break;
            }
        }

        private void ApplyConnectionRefusedKind()
        {
            SetTextKeys("ErrorDialog_LblTitleConnectionRefused", "Error_NetConnectionRefused");
            btnRetry.Visibility = Visibility.Visible;
        }

        private void ApplyPlayerBusyKind()
        {
            SetTextKeys("ErrorDialog_LblTitlePlayerBusy", "Error_GamePlayerBusy");
            btnReturnToMatch.Visibility = Visibility.Visible;
        }

        private void SetTextKeys(string titleKey, string messageKey)
        {
            _titleKey = titleKey;
            _messageKey = messageKey;
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
