using System.Collections.Generic;

namespace GinRummy.Client.Models
{
    /// <summary>
    /// Close of a hand in which the opponent knocked and the player defended, as the table
    /// explains it: who wins the hand, how the points are counted, the groups of both hands and
    /// the score of the match afterwards.
    /// </summary>
    public sealed class HandResultDto
    {
        /// <summary>
        /// Gets or sets the name of the player who wins the hand.
        /// </summary>
        public string WinnerName { get; set; }
        /// <summary>
        /// Gets or sets the name of the player who knocked.
        /// </summary>
        public string KnockerName { get; set; }
        /// <summary>
        /// Gets or sets the deadwood with which the hand was knocked.
        /// </summary>
        public int KnockerDeadwood { get; set; }
        /// <summary>
        /// Gets or sets the deadwood the player kept after laying off.
        /// </summary>
        public int DefenderDeadwood { get; set; }
        /// <summary>
        /// Gets or sets the points the winner of the hand earns.
        /// </summary>
        public int PointsAwarded { get; set; }
        /// <summary>
        /// Gets or sets the groups and loose cards of the player who knocked.
        /// </summary>
        public IList<MeldDto> KnockerMelds { get; set; }
        /// <summary>
        /// Gets or sets the groups and loose cards of the player.
        /// </summary>
        public IList<MeldDto> DefenderMelds { get; set; }
        /// <summary>
        /// Gets or sets the score of the player once the hand is counted.
        /// </summary>
        public int PlayerScore { get; set; }
        /// <summary>
        /// Gets or sets the score of the opponent once the hand is counted.
        /// </summary>
        public int OpponentScore { get; set; }
    }
}
