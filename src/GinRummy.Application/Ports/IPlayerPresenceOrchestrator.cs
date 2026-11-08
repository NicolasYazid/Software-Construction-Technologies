using System.Collections.Generic;

using GinRummy.Application.Presence;
using GinRummy.Domain.Entities.PlayerPresence;

namespace GinRummy.Application.Ports
{
    public interface IPlayerPresenceOrchestrator
    {
        int IdentifyByAccessToken(string accessToken);

        void AnnouncePlayerStatus(
            int playerWithNewStatusId,
            PlayerPresenceStatus newStatus,
            IList<OnlinePlayer> onlinePlayers);

        void ReportCurrentPresenceStatusOfPlayers(int subscriberId, IList<OnlinePlayer> onlinePlayers);
    }
}
