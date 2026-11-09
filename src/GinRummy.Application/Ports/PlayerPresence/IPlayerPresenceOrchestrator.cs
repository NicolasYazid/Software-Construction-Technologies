using System.Collections.Generic;

using GinRummy.Application.UseCases.PlayerPresence;
using GinRummy.Domain.Entities.Authentication;
using GinRummy.Domain.Entities.PlayerPresence;

namespace GinRummy.Application.Ports.PlayerPresence
{
    public interface IPlayerPresenceOrchestrator
    {
        AccessTokenOwner IdentifyByAccessToken(string accessToken);

        void AnnouncePlayerStatus(
            AccessTokenOwner playerWithNewStatus,
            PlayerPresenceStatus newStatus,
            IList<OnlinePlayer> onlinePlayers);

        void ReportCurrentPresenceStatusOfPlayers(AccessTokenOwner subscriber, IList<OnlinePlayer> onlinePlayers);
    }
}
