namespace GinRummy.Client.Models
{
    public enum ErrorDialogKind
    {
        ServiceUnavailable,
        ConnectionRefused,
        Unexpected,
        ConnectionLost,
        SessionExpired,

        LogOutNotRecorded,

        SessionClosedElsewhere,

        PlayerBusy,
        DataNotFound,
        MatchFinished
    }
}
