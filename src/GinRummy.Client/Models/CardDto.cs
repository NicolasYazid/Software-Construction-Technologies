namespace GinRummy.Client.Models
{
    // Card as the game table draws it, in a hand, on the discard pile or in a meld.
    public sealed class CardDto
    {
        public CardRank Rank { get; set; }
        public CardSuit Suit { get; set; }
    }
}
