using System;
using System.Collections.Generic;
using System.Linq;

namespace GinRummy.Domain
{
    public static class MeldFinder
    {
        private const int MinMeldSize = 3;

        public static IList<IList<Card>> FindBestMelds(IList<Card> cards)
        {
            if (cards == null)
            {
                throw new ArgumentNullException(nameof(cards));
            }

            return FindBestCombination(FindCandidateMelds(cards), 0, new HashSet<Card>());
        }

        public static int CountDeadwood(IList<Card> cards)
        {
            List<Card> meldedCards = FindBestMelds(cards).SelectMany(meld => meld).ToList();

            return cards.Where(card => !meldedCards.Contains(card)).Sum(card => card.DeadwoodValue);
        }

        private static List<List<Card>> FindCandidateMelds(IList<Card> cards)
        {
            List<List<Card>> candidates = new List<List<Card>>();
            foreach (IGrouping<int, Card> sameRankCards in cards.GroupBy(card => card.Rank))
            {
                AddSets(candidates, sameRankCards.ToList());
            }

            foreach (IGrouping<CardSuit, Card> sameSuitCards in cards.GroupBy(card => card.Suit))
            {
                AddRuns(candidates, sameSuitCards.OrderBy(card => card.Rank).ToList());
            }

            return candidates;
        }

        private static void AddSets(List<List<Card>> candidates, List<Card> sameRankCards)
        {
            if (sameRankCards.Count >= MinMeldSize)
            {
                candidates.Add(sameRankCards);
            }

            if (sameRankCards.Count > MinMeldSize)
            {
                foreach (Card leftOutCard in sameRankCards)
                {
                    candidates.Add(sameRankCards.Where(card => !card.Equals(leftOutCard)).ToList());
                }
            }
        }

        private static void AddRuns(List<List<Card>> candidates, List<Card> sameSuitCards)
        {
            for (int firstIndex = 0; firstIndex < sameSuitCards.Count; firstIndex++)
            {
                int lastIndex = firstIndex;
                while ((lastIndex + 1 < sameSuitCards.Count) && (sameSuitCards[lastIndex + 1].Rank == sameSuitCards[lastIndex].Rank + 1))
                {
                    lastIndex++;
                    if (lastIndex - firstIndex + 1 >= MinMeldSize)
                    {
                        candidates.Add(sameSuitCards.GetRange(firstIndex, lastIndex - firstIndex + 1));
                    }
                }
            }
        }

        private static List<IList<Card>> FindBestCombination(List<List<Card>> candidates, int firstIndex, HashSet<Card> usedCards)
        {
            List<IList<Card>> bestMelds = new List<IList<Card>>();
            int bestPoints = 0;
            for (int index = firstIndex; index < candidates.Count; index++)
            {
                List<Card> candidateMeld = candidates[index];
                if (!candidateMeld.Any(usedCards.Contains))
                {
                    usedCards.UnionWith(candidateMeld);
                    List<IList<Card>> melds = FindBestCombination(candidates, index + 1, usedCards);
                    usedCards.ExceptWith(candidateMeld);
                    melds.Insert(0, candidateMeld);
                    int points = CountPoints(melds);
                    if (points > bestPoints)
                    {
                        bestPoints = points;
                        bestMelds = melds;
                    }
                }
            }

            return bestMelds;
        }

        private static int CountPoints(IList<IList<Card>> melds)
        {
            return melds.SelectMany(meld => meld).Sum(card => card.DeadwoodValue);
        }
    }
}
