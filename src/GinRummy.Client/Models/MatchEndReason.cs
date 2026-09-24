namespace GinRummy.Client.Models
{
    /// <summary>
    /// Reason a match ended, seen from the player who looks at the table. It decides both the
    /// verdict and the sentence that explains it.
    /// </summary>
    public enum MatchEndReason
    {
        /// <summary>
        /// The player reached the target score: a victory.
        /// </summary>
        PlayerReachedTarget,

        /// <summary>
        /// The opponent forfeited the match: a victory (CU-26).
        /// </summary>
        OpponentForfeited,

        /// <summary>
        /// The opponent reached the target score: a defeat.
        /// </summary>
        OpponentReachedTarget
    }
}
