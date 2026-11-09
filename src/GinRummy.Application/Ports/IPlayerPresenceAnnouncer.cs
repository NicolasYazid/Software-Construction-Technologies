using GinRummy.Application.Presence;
using GinRummy.Domain.Entities.Authentication;

namespace GinRummy.Application.Ports
{
    public interface IPlayerPresenceAnnouncer
    {
        void SendCurrentPresenceStatusOfPlayers(AccessTokenOwner playerToNotify, PlayersByStatus lists);

        void SendPlayerPresenceChange(AccessTokenOwner playerToNotify, PlayerPresenceChange change);
    }
}
