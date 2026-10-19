using System.Runtime.Serialization;

namespace GinRummy.Contracts.Game
{
    /// <summary>
    /// A move the opponent made, for the match log. The table state that follows it arrives in OnTurnChanged.
    /// </summary>
    [DataContract]
    public class OpponentMoveDto
    {
        /// <summary>
        /// Gets or sets the move the opponent made.
        /// </summary>
        [DataMember]
        public OpponentMoveKind Kind { get; set; }

        /// <summary>
        /// Gets or sets the card involved; null when the move was drawing from the stock, because that card is
        /// hidden from the other player.
        /// </summary>
        [DataMember]
        public CardDto Card { get; set; }
    }
}