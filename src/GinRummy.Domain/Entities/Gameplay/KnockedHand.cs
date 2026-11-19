using System.Collections.Generic;
using System.Linq;

namespace GinRummy.Domain
{
    public class KnockedHand
    {
        private const int GinDeadwood = 0;

        private readonly List<List<Card>> _melds;
        private List<Card> _defenderUnmatchedCards;

        public KnockedHand(int knockerId, int deadwood, IList<IList<Card>> melds)
        {
            KnockerId = knockerId;
            Deadwood = deadwood;
            _melds = melds.Select(meld => meld.ToList()).ToList();
        }

        public int KnockerId { get; private set; }
        public int Deadwood { get; private set; }
        public int DefenderDeadwood { get; private set; }

        public IList<IList<Card>> Melds
        {
            get { return _melds.Select(meld => (IList<Card>)meld.OrderBy(card => card.Rank).ToList()).ToList(); }
        }

        public IReadOnlyList<Card> DefenderUnmatchedCards
        {
            get { return _defenderUnmatchedCards.AsReadOnly(); }
        }

        public RoundResult ResolveDefense(int defenderId, List<Card> unmatchedCards)
        {
            if (Deadwood > GinDeadwood)
            {
                LayOff(unmatchedCards);
            }

            _defenderUnmatchedCards = unmatchedCards;
            DefenderDeadwood = unmatchedCards.Sum(card => card.DeadwoodValue);

            return Score(defenderId);
        }

        private void LayOff(List<Card> unmatchedCards)
        {
            bool hasLaidOffCard = true;
            while (hasLaidOffCard)
            {
                hasLaidOffCard = false;
                foreach (Card card in unmatchedCards.ToList())
                {
                    List<Card> meld = _melds.FirstOrDefault(knockerMeld => MeldRules.CanLayOff(card, knockerMeld));
                    if (meld != null)
                    {
                        meld.Add(card);
                        unmatchedCards.Remove(card);
                        hasLaidOffCard = true;
                    }
                }
            }
        }

        private RoundResult Score(int defenderId)
        {
            RoundResult result;
            if (Deadwood == GinDeadwood)
            {
                result = new RoundResult(KnockerId, DefenderDeadwood + RoundRules.GinBonus, RoundOutcome.Gin);
            }
            else if (DefenderDeadwood <= Deadwood)
            {
                int points = (Deadwood - DefenderDeadwood) + RoundRules.UndercutBonus;
                result = new RoundResult(defenderId, points, RoundOutcome.Undercut);
            }
            else
            {
                result = new RoundResult(KnockerId, DefenderDeadwood - Deadwood, RoundOutcome.Knock);
            }

            return result;
        }
    }
}
