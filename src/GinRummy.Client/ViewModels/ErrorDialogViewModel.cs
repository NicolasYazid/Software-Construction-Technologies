using GinRummy.Client.Mvvm;

namespace GinRummy.Client.ViewModels
{
    public class ErrorDialogViewModel : ViewModelBase
    {
        private const string ServiceUnavailableTitleKey = "ErrorDialog_LblTitleServiceUnavailable";
        private const string ServiceUnavailableMessageKey = "Error_SysServiceUnavailable";
        private const string ConnectionRefusedTitleKey = "ErrorDialog_LblTitleConnectionRefused";
        private const string ConnectionRefusedMessageKey = "Error_NetConnectionRefused";
        private const string UnexpectedTitleKey = "ErrorDialog_LblTitleUnexpected";
        private const string UnexpectedMessageKey = "Error_SysUnexpected";
        private const string ConnectionLostTitleKey = "ErrorDialog_LblTitleConnectionLost";
        private const string ConnectionLostMessageKey = "Error_NetConnectionLost";
        private const string SessionExpiredTitleKey = "ErrorDialog_LblTitleSessionExpired";
        private const string SessionExpiredMessageKey = "Error_AuthSessionExpired";
        private const string LogOutNotRecordedTitleKey = "ErrorDialog_LblTitleLogOutNotRecorded";
        private const string LogOutNotRecordedMessageKey = "Error_SysLogOutNotRecorded";
        private const string SessionClosedElsewhereTitleKey = "ErrorDialog_LblTitleSessionClosedElsewhere";
        private const string SessionClosedElsewhereMessageKey = "LogIn_SessionClosedElsewhere";
        private const string PlayerBusyTitleKey = "ErrorDialog_LblTitlePlayerBusy";
        private const string PlayerBusyMessageKey = "Error_GamePlayerBusy";
        private const string DataNotFoundTitleKey = "ErrorDialog_LblTitleDataNotFound";
        private const string DataNotFoundMessageKey = "Error_DataNotFound";

        public ErrorDialogViewModel(ErrorDialogKind kind)
        {
            ApplyKind(kind);
        }

        public string TitleKey { get; private set; }
        public string MessageKey { get; private set; }
        public bool IsRetryOffered { get; private set; }
        public bool IsReturnToMatchOffered { get; private set; }

        private void ApplyKind(ErrorDialogKind kind)
        {
            switch (kind)
            {
                case ErrorDialogKind.ConnectionRefused:
                    SetTextKeys(ConnectionRefusedTitleKey, ConnectionRefusedMessageKey);
                    IsRetryOffered = true;
                    break;
                case ErrorDialogKind.Unexpected:
                    SetTextKeys(UnexpectedTitleKey, UnexpectedMessageKey);
                    break;
                case ErrorDialogKind.ConnectionLost:
                    SetTextKeys(ConnectionLostTitleKey, ConnectionLostMessageKey);
                    break;
                case ErrorDialogKind.SessionExpired:
                    SetTextKeys(SessionExpiredTitleKey, SessionExpiredMessageKey);
                    break;
                case ErrorDialogKind.LogOutNotRecorded:
                    SetTextKeys(LogOutNotRecordedTitleKey, LogOutNotRecordedMessageKey);
                    break;
                case ErrorDialogKind.SessionClosedElsewhere:
                    SetTextKeys(SessionClosedElsewhereTitleKey, SessionClosedElsewhereMessageKey);
                    break;
                case ErrorDialogKind.PlayerBusy:
                    SetTextKeys(PlayerBusyTitleKey, PlayerBusyMessageKey);
                    IsReturnToMatchOffered = true;
                    break;
                case ErrorDialogKind.DataNotFound:
                    SetTextKeys(DataNotFoundTitleKey, DataNotFoundMessageKey);
                    break;
                default:
                    SetTextKeys(ServiceUnavailableTitleKey, ServiceUnavailableMessageKey);
                    break;
            }
        }

        private void SetTextKeys(string titleKey, string messageKey)
        {
            TitleKey = titleKey;
            MessageKey = messageKey;
        }
    }
}
