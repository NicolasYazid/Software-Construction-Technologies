using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace GinRummy.Client.Models
{
    public static class HandOrder
    {
        public static IList<Card> SortByRank(IEnumerable<Card> hand)
        {
            return hand.OrderBy(card => card.Rank).ThenBy(card => card.Suit).ToList();
        }

        public static IList<Card> SortLooseCards(IList<Card> hand, IEnumerable<IList<Card>> melds)
        {
            List<Card> meldCards = melds.OrderBy(meld => meld.Min(hand.IndexOf)).SelectMany(meld => meld.OrderBy(hand.IndexOf)).ToList();

            return meldCards.Concat(SortByRank(hand.Except(meldCards))).ToList();
        }

        public static IList<Card> PutMeldsFirst(IEnumerable<Card> hand, IList<Card> meldCards)
        {
            return meldCards.Concat(hand.Except(meldCards)).ToList();
        }

        public static void Arrange(ObservableCollection<Card> hand, IList<Card> order)
        {
            for (int index = 0; index < order.Count; index++)
            {
                int currentIndex = hand.IndexOf(order[index]);
                if (currentIndex != index)
                {
                    hand.Move(currentIndex, index);
                }
            }
        }

        public static IList<IList<Card>> SplitIntoNeighbors(IList<Card> hand, IEnumerable<Card> cards)
        {
            List<IList<Card>> stretches = new List<IList<Card>>();
            List<Card> stretch = null;
            int previousIndex = 0;
            foreach (Card card in cards.Where(hand.Contains).OrderBy(hand.IndexOf))
            {
                int index = hand.IndexOf(card);
                if ((stretch == null) || (index != previousIndex + 1))
                {
                    stretch = new List<Card>();
                    stretches.Add(stretch);
                }

                stretch.Add(card);
                previousIndex = index;
            }

            return stretches;
        }
    }
}