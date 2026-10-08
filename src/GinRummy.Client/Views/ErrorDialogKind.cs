namespace GinRummy.Client.Views
{
    // The error dialog (P22) knows how to present these nine errors.
    // Each one fixes the title, the message and whether a second action is offered beside the one that closes it.
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
