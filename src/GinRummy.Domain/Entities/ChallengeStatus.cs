namespace GinRummy.Domain.Entities
{
    /// <summary>
    /// Where a challenge stands. The names match the values the status column of the Challenge
    /// table accepts.
    /// </summary>
    public enum ChallengeStatus
    {
        /// <summary>
        /// The challenge was sent and nobody has answered it yet.
        /// </summary>
        Sent,
        /// <summary>
        /// The challenged player accepted and a match was created.
        /// </summary>
        Accepted,
        /// <summary>
        /// The challenged player rejected it.
        /// </summary>
        Declined,
        /// <summary>
        /// The challenger took it back before it was answered.
        /// </summary>
        Cancelled,
        /// <summary>
        /// Nobody answered before its expiry moment.
        /// </summary>
        Expired
    }
}
