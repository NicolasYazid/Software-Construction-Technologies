namespace GinRummy.Client.Views
{
    /// <summary>
    /// The nine errors the error dialog (P22) knows how to present. Each one fixes the title,
    /// the message and whether a second action is offered beside the one that closes it.
    /// </summary>
    public enum ErrorDialogKind
    {
        /// <summary>
        /// The database could not be reached (DAT-01).
        /// </summary>
        ServiceUnavailable,

        /// <summary>
        /// The server refused the connection (NET-02). Offers a retry.
        /// </summary>
        ConnectionRefused,

        /// <summary>
        /// A transaction failed after its retry (DAT-04).
        /// </summary>
        Unexpected,

        /// <summary>
        /// The connection with the server was lost (NET-03).
        /// </summary>
        ConnectionLost,

        /// <summary>
        /// The session of the player is no longer valid (AUT-07).
        /// </summary>
        SessionExpired,

        /// <summary>
        /// The session was closed on the client but could not be recorded (CU-03 EX-01).
        /// </summary>
        LogOutNotRecorded,

        /// <summary>
        /// The account was opened on another device (CU-02 FA-03).
        /// </summary>
        SessionClosedElsewhere,

        /// <summary>
        /// The player already takes part in a match (GAM-14). Offers to return to it.
        /// </summary>
        PlayerBusy,

        /// <summary>
        /// The player that was looked for no longer exists (DAT-05).
        /// </summary>
        DataNotFound
    }
}
