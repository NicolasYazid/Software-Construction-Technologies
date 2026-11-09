using GinRummy.Domain.Entities.Authentication;
using GinRummy.Domain.Entities.PlayerPresence;

namespace GinRummy.Application.Presence
{
    public class OnlinePlayer
    {
        public OnlinePlayer(AccessTokenOwner accessTokenOwner, PlayerPresenceStatus status)
        {
            AccessTokenOwner = accessTokenOwner;
            Status = status;
        }

        public AccessTokenOwner AccessTokenOwner { get; private set; }

        public PlayerPresenceStatus Status { get; private set; }
    }
}
