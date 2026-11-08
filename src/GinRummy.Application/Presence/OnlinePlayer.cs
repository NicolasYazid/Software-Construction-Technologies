using GinRummy.Domain.Entities.PlayerPresence;

namespace GinRummy.Application.Presence
{
    public class OnlinePlayer
    {
        public OnlinePlayer(int playerId, PlayerPresenceStatus status)
        {
            PlayerId = playerId;
            Status = status;
        }

        public int PlayerId { get; private set; }

        public PlayerPresenceStatus Status { get; private set; }
    }
}
