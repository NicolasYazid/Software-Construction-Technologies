using GinRummy.Application.UseCases.PlayerPresence;
using GinRummy.Domain.Entities.Authentication;

namespace GinRummy.Application.Ports.PlayerPresence
{
    public interface IPlayerPresenceAnnouncer
    {
        void SendCurrentPresenceStatusOfPlayers(AccessTokenOwner playerToNotify, PlayersByStatus lists);

        void SendPlayerPresenceChange(AccessTokenOwner playerToNotify, PlayerPresenceChange change);
    }
}
