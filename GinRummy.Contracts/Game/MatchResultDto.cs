using System.Runtime.Serialization;

namespace GinRummy.Contracts.Game
{
    /// <summary>
    /// Final result of a match, built for the player who receives it.
    /// </summary>
    [DataContract]
    public class MatchResultDto
    {
        /// <summary>
        /// Gets or sets why the match ended, from the player's point of view.
        /// </summary>
        [DataMember]
        public MatchEndReason EndReason { get; set; }

        /// <summary>
        /// Gets or sets the final score of the player.
        /// </summary>
        [DataMember]
        public int YourScore { get; set; }

        /// <summary>
        /// Gets or sets the final score of the opponent.
        /// </summary>
        [DataMember]
        public int OpponentScore { get; set; }
    }
}