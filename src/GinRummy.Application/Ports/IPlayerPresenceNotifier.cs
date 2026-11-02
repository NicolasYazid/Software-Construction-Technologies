using GinRummy.Application.Presence;
using GinRummy.Domain.Entities.Players;

namespace GinRummy.Application.Ports
{
    public interface IPlayerPresenceNotifier
    {
        void SendCurrentStatusOfPlayers(int playerId, PlayerPresenceLists lists);

        void SendPlayerPresenceChange(ConnectedPlayer recipient, Player changedPlayer, PlayerPresenceStatus status);
    }
}
