namespace GinRummy.Client.ViewModels
{
    public enum ErrorDialogKind
    {
        ServiceUnavailable,
        ConnectionRefused,
        Unexpected,
        ConnectionLost,
        SessionExpired,

        /// <summary>
        /// The session was closed on the client but could not be recorded (CU-03 EX-01).
        /// </summary>
        LogOutNotRecorded,

        /// <summary>
        /// The account was opened on another device (CU-02 FA-03).
        /// </summary>
        SessionClosedElsewhere,

        PlayerBusy,
        DataNotFound
    }
}
