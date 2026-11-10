using System;
using System.Collections.Generic;
using System.Linq;

namespace GinRummy.Domain.Gameplay
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
            foreach (IGrouping<int, Card> sameRank in cards.GroupBy(card => card.Rank))
            {
                AddSets(candidates, sameRank.ToList());
            }

            foreach (IGrouping<CardSuit, Card> sameSuit in cards.GroupBy(card => card.Suit))
            {
                AddRuns(candidates, sameSuit.OrderBy(card => card.Rank).ToList());
            }

            return candidates;
        }

        private static void AddSets(List<List<Card>> candidates, List<Card> sameRank)
        {
            if (sameRank.Count >= MinMeldSize)
            {
                candidates.Add(sameRank);
            }

            if (sameRank.Count > MinMeldSize)
            {
                foreach (Card leftOutCard in sameRank)
                {
                    candidates.Add(sameRank.Where(card => !card.Equals(leftOutCard)).ToList());
                }
            }
        }

        private static void AddRuns(List<List<Card>> candidates, List<Card> sameSuit)
        {
            for (int firstIndex = 0; firstIndex < sameSuit.Count; firstIndex++)
            {
                int lastIndex = firstIndex;
                while ((lastIndex + 1 < sameSuit.Count) && (sameSuit[lastIndex + 1].Rank == sameSuit[lastIndex].Rank + 1))
                {
                    lastIndex++;
                    if (lastIndex - firstIndex + 1 >= MinMeldSize)
                    {
                        candidates.Add(sameSuit.GetRange(firstIndex, lastIndex - firstIndex + 1));
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
                List<Card> candidate = candidates[index];
                if (!candidate.Any(usedCards.Contains))
                {
                    usedCards.UnionWith(candidate);
                    List<IList<Card>> melds = FindBestCombination(candidates, index + 1, usedCards);
                    usedCards.ExceptWith(candidate);
                    melds.Insert(0, candidate);
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
