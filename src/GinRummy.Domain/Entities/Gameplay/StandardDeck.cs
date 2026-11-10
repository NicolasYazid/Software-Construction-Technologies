using System;
using System.Collections.Generic;

namespace GinRummy.Domain
{
    public static class StandardDeck
    {
        public const int CardCount = 52;

        public static IList<Card> CreateOrdered()
        {
            List<Card> cards = new List<Card>(CardCount);
            foreach (CardSuit suit in Enum.GetValues(typeof(CardSuit)))
            {
                for (int rank = Card.AceRank; rank <= Card.KingRank; rank++)
                {
                    cards.Add(new Card(suit, rank));
                }
            }

            return cards;
        }

        public static IList<Card> CreateShuffled(Random random)
        {
            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            IList<Card> cards = CreateOrdered();
            for (int index = cards.Count - 1; index > 0; index--)
            {
                int swapIndex = random.Next(index + 1);
                Card card = cards[index];
                cards[index] = cards[swapIndex];
                cards[swapIndex] = card;
            }

            return cards;
        }
    }
}
