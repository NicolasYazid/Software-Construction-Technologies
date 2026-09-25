namespace GinRummy.Client.Models
{
    /// <summary>
    /// Kind of notice a challenge leaves in the lobby chat.
    /// </summary>
    public enum ChallengeNoticeKind
    {
        /// <summary>
        /// The player challenged someone and waits for the answer (CU-24).
        /// </summary>
        Sent,

        /// <summary>
        /// The challenged player declined the challenge of the player (CU-25).
        /// </summary>
        Declined,

        /// <summary>
        /// Another player challenged the player (CU-23).
        /// </summary>
        Received
    }
}
