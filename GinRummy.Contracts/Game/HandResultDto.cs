using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GinRummy.Contracts.Game
{
    /// <summary>
    /// Result of a hand, built for the player who receives it. When a hand closes, both hands are shown, so here
    /// the opponent's cards may travel.
    /// </summary>
    [DataContract]
    public class HandResultDto
    {
        /// <summary>
        /// Gets or sets how the hand ended.
        /// </summary>
        [DataMember]
        public HandOutcome Outcome { get; set; }

        /// <summary>
        /// Gets or sets whether the player won the hand; false for both players when the hand is void.
        /// </summary>
        [DataMember]
        public bool IsWinner { get; set; }

        /// <summary>
        /// Gets or sets whether the player is the one who closed the hand.
        /// </summary>
        [DataMember]
        public bool HasClosedHand { get; set; }

        /// <summary>
        /// Gets or sets the melds of the player who closed, arranged by the server for the least deadwood.
        /// </summary>
        [DataMember]
        public List<MeldDto> CloserMelds { get; set; }

        /// <summary>
        /// Gets or sets the unmatched cards of the player who closed; empty after a gin.
        /// </summary>
        [DataMember]
        public List<CardDto> CloserDeadwood { get; set; }

        /// <summary>
        /// Gets or sets the melds of the defender.
        /// </summary>
        [DataMember]
        public List<MeldDto> DefenderMelds { get; set; }

        /// <summary>
        /// Gets or sets the cards of the defender that the server laid off on the melds of the player who closed;
        /// always empty after a gin (house rules 9 and 10).
        /// </summary>
        [DataMember]
        public List<CardDto> LaidOffCards { get; set; }

        /// <summary>
        /// Gets or sets the unmatched cards the defender kept after the lay-off.
        /// </summary>
        [DataMember]
        public List<CardDto> DefenderDeadwood { get; set; }

        /// <summary>
        /// Gets or sets the deadwood points of the player who closed.
        /// </summary>
        [DataMember]
        public int CloserDeadwoodPoints { get; set; }

        /// <summary>
        /// Gets or sets the deadwood points of the defender after the lay-off.
        /// </summary>
        [DataMember]
        public int DefenderDeadwoodPoints { get; set; }

        /// <summary>
        /// Gets or sets the points the winner of the hand earned, bonuses included; zero when the hand is void.
        /// </summary>
        [DataMember]
        public int PointsAwarded { get; set; }

        /// <summary>
        /// Gets or sets the match score of the player after this hand.
        /// </summary>
        [DataMember]
        public int YourScore { get; set; }

        /// <summary>
        /// Gets or sets the match score of the opponent after this hand.
        /// </summary>
        [DataMember]
        public int OpponentScore { get; set; }
    }
}