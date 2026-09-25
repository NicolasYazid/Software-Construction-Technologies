namespace GinRummy.Client.Models
{
    /// <summary>
    /// Card as the game table draws it, in a hand, on the discard pile or in a meld.
    /// </summary>
    public sealed class CardDto
    {
        /// <summary>
        /// Gets or sets the rank of the card.
        /// </summary>
        public CardRank Rank { get; set; }
        /// <summary>
        /// Gets or sets the suit of the card.
        /// </summary>
        public CardSuit Suit { get; set; }
    }
}
