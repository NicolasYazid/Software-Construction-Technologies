using System;

using GinRummy.Domain.Entities.Players;

namespace GinRummy.Application.Presence
{
    public class PlayerPresenceChange
    {
        public PlayerPresenceChange(Player player, PlayerPresenceStatus status, bool isFriend)
        {
            Player = player ?? throw new ArgumentNullException(nameof(player));
            Status = status;
            IsFriend = isFriend;
        }

        public Player Player { get; private set; }

        public PlayerPresenceStatus Status { get; private set; }

        public bool IsFriend { get; private set; }
    }
}
