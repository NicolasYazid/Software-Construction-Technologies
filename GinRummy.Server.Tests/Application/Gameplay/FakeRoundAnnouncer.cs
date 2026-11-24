using System.Collections.Generic;

using GinRummy.Application.Ports.Gameplay;
using GinRummy.Application.UseCases.Gameplay;

namespace GinRummy.Server.Tests.Application.Gameplay
{
    public class FakeRoundAnnouncer : IRoundAnnouncer
    {
        public FakeRoundAnnouncer()
        {
            ChangedPlayerIds = new List<int>();
            FinishedPlayerIds = new List<int>();
            LeftPlayerIds = new List<int>();
            DisconnectedNoticePlayerIds = new List<int>();
            ReconnectedNoticePlayerIds = new List<int>();
            OfflinePlayerIds = new List<int>();
        }

        public IList<int> ChangedPlayerIds { get; private set; }
        public IList<int> FinishedPlayerIds { get; private set; }
        public IList<int> LeftPlayerIds { get; private set; }
        public IList<int> DisconnectedNoticePlayerIds { get; private set; }
        public IList<int> ReconnectedNoticePlayerIds { get; private set; }
        public IList<int> OfflinePlayerIds { get; private set; }

        public void AnnounceRoundStarted(int playerId, ActiveMatch match)
        {
        }

        public void AnnounceRoundChanged(int playerId, ActiveMatch match)
        {
            ChangedPlayerIds.Add(playerId);
        }

        public void AnnounceRoundFinished(int playerId, ActiveMatch match)
        {
            FinishedPlayerIds.Add(playerId);
        }

        public void AnnounceOpponentLeft(int playerId)
        {
            LeftPlayerIds.Add(playerId);
        }

        public void AnnounceOpponentDisconnected(int playerId)
        {
            DisconnectedNoticePlayerIds.Add(playerId);
        }

        public void AnnounceOpponentReconnected(int playerId)
        {
            ReconnectedNoticePlayerIds.Add(playerId);
        }

        public bool IsPlayerOnline(int playerId)
        {
            return !OfflinePlayerIds.Contains(playerId) ? true : false;
        }
    }
}
