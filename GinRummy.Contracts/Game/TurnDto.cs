using System.Runtime.Serialization;

namespace GinRummy.Contracts.Game
{
    /// <summary>
    /// State of the table after a move, built for the player who receives it. It is sent to both players after
    /// every accepted move, so it also confirms the player's own move.
    /// </summary>
    [DataContract]
    public class TurnDto
    {
        /// <summary>
        /// Gets or sets whether the player who receives the state must make the next move.
        /// </summary>
        [DataMember]
        public bool IsYourTurn { get; set; }

        /// <summary>
        /// Gets or sets the step of the turn that comes next.
        /// </summary>
        [DataMember]
        public TurnPhase Phase { get; set; }

        /// <summary>
        /// Gets or sets how many cards are left in the stock.
        /// </summary>
        [DataMember]
        public int StockCount { get; set; }

        /// <summary>
        /// Gets or sets the top card of the discard pile; null when the pile is empty.
        /// </summary>
        [DataMember]
        public CardDto DiscardTop { get; set; }
    }
}