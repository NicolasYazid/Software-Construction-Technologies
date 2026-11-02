using System;
using System.Collections.Generic;

using GinRummy.Domain.Entities.Players;

namespace GinRummy.Application.Presence
{
    public class ConnectedPlayer
    {
        private readonly IList<int> _friendPlayerIds;

        public ConnectedPlayer(Player player, IList<int> friendPlayerIds)
        {
            Player = player ?? throw new ArgumentNullException(nameof(player));
            _friendPlayerIds = friendPlayerIds ?? throw new ArgumentNullException(nameof(friendPlayerIds));
        }

        public Player Player { get; private set; }

        public bool IsFriendOf(int playerId)
        {
            bool isFriend = _friendPlayerIds.Contains(playerId);

            return isFriend;
        }
    }
}