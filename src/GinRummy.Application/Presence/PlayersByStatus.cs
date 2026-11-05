using System.Collections.Generic;

namespace GinRummy.Application.Presence
{
    public class PlayersByStatus
    {
        public PlayersByStatus(IList<RankedPlayer> players, IList<int> onlinePlayerIds, IList<int> friendPlayerIds)
        {
            AllOnlinePlayers = new List<RankedPlayer>();
            AllOfflinePlayers = new List<RankedPlayer>();
            AllOnlineFriends = new List<RankedPlayer>();
            AllOfflineFriends = new List<RankedPlayer>();

            foreach (RankedPlayer player in players)
            {
                bool isOnline = onlinePlayerIds.Contains(player.IdentityId);
                bool isFriend = friendPlayerIds.Contains(player.IdentityId);
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
