using System.Collections.Generic;

using GinRummy.Domain.Entities.Players;

namespace GinRummy.Application.Presence
{
    public class PlayersByStatus
    {
        public PlayersByStatus(IList<Player> players, IList<int> onlinePlayerIds, IList<int> friendPlayerIds)
        {
            AllOnlinePlayers = new List<Player>();
            AllOfflinePlayers = new List<Player>();
            AllOnlineFriends = new List<Player>();
            AllOfflineFriends = new List<Player>();

            foreach (Player player in players)
            {
                bool isOnline = onlinePlayerIds.Contains(player.PlayerId);
                bool isFriend = friendPlayerIds.Contains(player.PlayerId);
                AddPlayer(player, isOnline, isFriend);
            }
        }

        public IList<Player> AllOnlinePlayers { get; private set; }

        public IList<Player> AllOfflinePlayers { get; private set; }

        public IList<Player> AllOnlineFriends { get; private set; }

        public IList<Player> AllOfflineFriends { get; private set; }

        private void AddPlayer(Player player, bool isOnline, bool isFriend)
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
