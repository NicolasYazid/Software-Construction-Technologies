using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GinRummy.Contracts.Game
{
    /// <summary>
    /// What one player can see of the table: their own hand and the public state. It never carries the opponent's
    /// hand, only how many cards it has.
    /// </summary>
    [DataContract]
    public class GameTableDto
    {
        /// <summary>
        /// Gets or sets the match the table belongs to.
        /// </summary>
        [DataMember]
        public int GameSessionId { get; set; }

        /// <summary>
        /// Gets or sets the name the opponent shows at the table.
        /// </summary>
        [DataMember]
        public string OpponentUsername { get; set; }

        /// <summary>
        /// Gets or sets the cards in the player's hand.
        /// </summary>
        [DataMember]
        public List<CardDto> Hand { get; set; }

        /// <summary>
        /// Gets or sets the deadwood of the hand with its best arrangement, as the server computes it.
        /// </summary>
        [DataMember]
        public int Deadwood { get; set; }

        /// <summary>
        /// Gets or sets how many cards the opponent holds.
        /// </summary>
        [DataMember]
        public int OpponentCardCount { get; set; }

        /// <summary>
        /// Gets or sets whether the player dealt this hand; the dealer alternates every hand (house rule 14).
        /// </summary>
        [DataMember]
        public bool IsDealer { get; set; }

        /// <summary>
        /// Gets or sets whose turn it is, the step that comes next, the stock and the discard pile.
        /// </summary>
        [DataMember]
        public TurnDto Turn { get; set; }

        /// <summary>
        /// Gets or sets the match score of the player.
        /// </summary>
        [DataMember]
        public int YourScore { get; set; }

        /// <summary>
        /// Gets or sets the match score of the opponent.
        /// </summary>
        [DataMember]
        public int OpponentScore { get; set; }

        /// <summary>
        /// Gets or sets the score that wins the match (house rule 13).
        /// </summary>
        [DataMember]
        public int TargetScore { get; set; }

        /// <summary>
        /// Gets or sets whether the match is paused while a player reconnects (DEC-09).
        /// </summary>
        [DataMember]
        public bool IsPaused { get; set; }
    }
}