using GinRummy.Application.Presence;

namespace GinRummy.Application.Ports
{
    public interface IPlayerPresenceAnnouncer
    {
        void SendCurrentPresenceStatusOfPlayers(int playerToNotifyId, PlayersByStatus lists);

        void SendPlayerPresenceChange(int playerToNotifyId, PlayerPresenceChange change);
    }
}
