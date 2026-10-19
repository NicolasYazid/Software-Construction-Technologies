using System.Runtime.Serialization;

namespace GinRummy.Contracts.Game
{
    /// <summary>
    /// A card identified by its rank and suit. A deck has exactly one card per pair, so no identifier is needed, and
    /// the server checks that a card the client names is really in that player's hand.
    /// </summary>
    [DataContract]
    public class CardDto
    {
        /// <summary>
        /// Gets or sets the rank of the card.
        /// </summary>
        [DataMember]
        public CardRank Rank { get; set; }

        /// <summary>
        /// Gets or sets the suit of the card.
        /// </summary>
        [DataMember]
        public CardSuit Suit { get; set; }
    }
}