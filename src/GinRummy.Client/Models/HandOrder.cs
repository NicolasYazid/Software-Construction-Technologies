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

        public static IList<Card> PutMeldsFirst(IEnumerable<Card> hand, IList<Card> meldCards)
        {
            return meldCards.Concat(hand.Except(meldCards)).ToList();
        }

        // Each card is moved and not replaced, so the hand animates the cards to their new slots instead of dealing them again.
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
    }
}
