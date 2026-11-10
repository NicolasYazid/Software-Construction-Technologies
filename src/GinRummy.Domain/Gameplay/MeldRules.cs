using System.Collections.Generic;
using System.Linq;

namespace GinRummy.Domain.Gameplay
{
    public static class MeldRules
    {
        private const int MinMeldSize = 3;
        private const int MaxSetSize = 4;

        public static bool IsValidMeld(IList<Card> cards)
        {
            bool isValid = false;
            if ((cards != null) && (cards.Count >= MinMeldSize) && (cards.Distinct().Count() == cards.Count))
            {
                isValid = IsSet(cards) || IsRun(cards);
            }

            return isValid;
        }

        public static bool CanLayOff(Card card, IList<Card> meld)
        {
            List<Card> extendedMeld = new List<Card>(meld) { card };

            return IsValidMeld(extendedMeld);
        }

        private static bool IsSet(IList<Card> cards)
        {
            int firstRank = cards[0].Rank;

            return (cards.Count <= MaxSetSize) && cards.All(card => card.Rank == firstRank);
        }

        private static bool IsRun(IList<Card> cards)
        {
            CardSuit firstSuit = cards[0].Suit;
            bool isSameSuit = cards.All(card => card.Suit == firstSuit);
            int rankSpan = cards.Max(card => card.Rank) - cards.Min(card => card.Rank);

            return isSameSuit && (rankSpan == (cards.Count - 1));
        }
    }
}
