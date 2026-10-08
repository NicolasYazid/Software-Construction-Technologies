namespace GinRummy.Client.Views
{
    public enum ErrorDialogKind
    {
        ServiceUnavailable,
        ConnectionRefused,
        Unexpected,
        ConnectionLost,
        SessionExpired,
        // The session was closed on the client but could not be recorded (CU-03 EX-01).
        LogOutNotRecorded,
        // The account was opened on another device (CU-02 FA-03).
        SessionClosedElsewhere,
        PlayerBusy,
        DataNotFound
    }
}
