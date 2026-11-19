using System.Collections.Generic;

using GinRummy.Application.UseCases.DirectDuel;

namespace GinRummy.Application.UseCases.Gameplay
{
    public class MatchSnapshot
    {
        public MatchSnapshot(DuelPlayer dealer, DuelPlayer nonDealer, IDictionary<int, int> scoresByPlayerId)
        {
            Dealer = dealer;
            NonDealer = nonDealer;
            ScoresByPlayerId = scoresByPlayerId;
        }

        public DuelPlayer Dealer { get; private set; }

        public DuelPlayer NonDealer { get; private set; }

        public IDictionary<int, int> ScoresByPlayerId { get; private set; }
    }
}
