namespace GinRummy.Client.Models
{
    public sealed class Card
    {
        public Card(CardRank rank, CardSuit suit)
        {
            Rank = rank;
            Suit = suit;
        }

        public CardRank Rank { get; private set; }
        public CardSuit Suit { get; private set; }
    }
}
