using System;

using GinRummy.Domain.Entities.PlayerPresence;

namespace GinRummy.Application.Presence
{
    public class PlayerPresenceChange
    {
        public PlayerPresenceChange(RankedPlayer player, PlayerPresenceStatus status, bool isFriend)
        {
            Player = player ?? throw new ArgumentNullException(nameof(player));
            Status = status;
            IsFriend = isFriend;
        }

        public RankedPlayer Player { get; private set; }

        public PlayerPresenceStatus Status { get; private set; }

        public bool IsFriend { get; private set; }
    }
}
