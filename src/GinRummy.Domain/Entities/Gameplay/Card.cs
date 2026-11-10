using System;

namespace GinRummy.Domain
{
    public sealed class Card
    {
        public const int AceRank = 1;
        public const int KingRank = 13;
        private const int MaxDeadwoodValue = 10;

        public Card(CardSuit suit, int rank)
        {
            if ((rank < AceRank) || (rank > KingRank))
            {
                throw new ArgumentOutOfRangeException(nameof(rank), "The rank of a card goes from 1 to 13.");
            }

            Suit = suit;
            Rank = rank;
        }

        public CardSuit Suit { get; private set; }
        public int Rank { get; private set; }

        public int DeadwoodValue
        {
            get { return Math.Min(Rank, MaxDeadwoodValue); }
        }

        public override bool Equals(object obj)
        {
            bool isEqual = false;
            if (obj is Card card)
            {
                isEqual = (Suit == card.Suit) && (Rank == card.Rank);
            }

            return isEqual;
        }

        public override int GetHashCode()
        {
            return (Suit, Rank).GetHashCode();
        }
    }
}
