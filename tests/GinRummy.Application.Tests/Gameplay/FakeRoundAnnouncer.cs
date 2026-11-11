using System.Collections.Generic;

using GinRummy.Application.Ports.Gameplay;
using GinRummy.Application.UseCases.Gameplay;

namespace GinRummy.Application.Tests.Gameplay
{
    public class FakeRoundAnnouncer : IRoundAnnouncer
    {
        public FakeRoundAnnouncer()
        {
            ChangedPlayerIds = new List<int>();
            FinishedPlayerIds = new List<int>();
            LeftPlayerIds = new List<int>();
        }

        public IList<int> ChangedPlayerIds { get; private set; }
        public IList<int> FinishedPlayerIds { get; private set; }
        public IList<int> LeftPlayerIds { get; private set; }

        public void AnnounceRoundChanged(int playerId, ActiveRound round)
        {
            ChangedPlayerIds.Add(playerId);
        }

        public void AnnounceRoundFinished(int playerId, ActiveRound round)
        {
            FinishedPlayerIds.Add(playerId);
        }

        public void AnnounceOpponentLeft(int playerId)
        {
            LeftPlayerIds.Add(playerId);
        }
    }
}
