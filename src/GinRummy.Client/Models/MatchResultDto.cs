namespace GinRummy.Client.Models
{
    /// <summary>
    /// Result of a finished match, as the table announces it.
    /// </summary>
    public sealed class MatchResultDto
    {
        /// <summary>
        /// Gets or sets why the match ended.
        /// </summary>
        public MatchEndReason EndReason { get; set; }
        /// <summary>
        /// Gets or sets the final score of the player.
        /// </summary>
        public int PlayerScore { get; set; }
        /// <summary>
        /// Gets or sets the final score of the opponent.
        /// </summary>
        public int OpponentScore { get; set; }
    }
}
