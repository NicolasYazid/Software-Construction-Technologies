using System.Collections.Generic;

using GinRummy.Domain.Entities.Authentication;

namespace GinRummy.Application.UseCases.PlayerPresence
{
    public class PlayersByStatus
    {
        public PlayersByStatus(
            IList<RankedPlayer> players,
            IList<AccessTokenOwner> onlineOwners,
            IList<int> friendPlayerAccountIds)
        {
            AllOnlinePlayers = new List<RankedPlayer>();
            AllOfflinePlayers = new List<RankedPlayer>();
            AllOnlineFriends = new List<RankedPlayer>();
            AllOfflineFriends = new List<RankedPlayer>();

            foreach (RankedPlayer player in players)
            {
                bool isOnline = onlineOwners.Contains(player.AccessTokenOwner);
                bool isFriend = player.AccessTokenOwner.PlayerAccountId.HasValue
                    && friendPlayerAccountIds.Contains(player.AccessTokenOwner.PlayerAccountId.Value);
                AddPlayer(player, isOnline, isFriend);
            }
        }

        public IList<RankedPlayer> AllOnlinePlayers { get; private set; }

        public IList<RankedPlayer> AllOfflinePlayers { get; private set; }

        public IList<RankedPlayer> AllOnlineFriends { get; private set; }

        public IList<RankedPlayer> AllOfflineFriends { get; private set; }

        private void AddPlayer(RankedPlayer player, bool isOnline, bool isFriend)
        {
            if (isOnline)
            {
                AllOnlinePlayers.Add(player);
                if (isFriend)
                {
                    AllOnlineFriends.Add(player);
                }
            }
            else
            {
                AllOfflinePlayers.Add(player);
                if (isFriend)
                {
                    AllOfflineFriends.Add(player);
                }
            }
        }
    }
}
