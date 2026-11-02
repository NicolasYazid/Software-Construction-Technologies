using System.Collections.Generic;
using GinRummy.Domain.Entities.Players;

namespace GinRummy.Application.Presence
{
    public class PlayerPresenceLists
    {
        public PlayerPresenceLists(IList<Player> registeredPlayers, IList<int> connectedPlayerIds, ConnectedPlayer subscriber)
        {
            AllOnlinePlayers = new List<Player>();
            AllOfflinePlayers = new List<Player>();
            AllOnlineFriends = new List<Player>();
            AllOfflineFriends = new List<Player>();

            foreach (Player player in registeredPlayers)
            {
                if (player.PlayerId != subscriber.Player.PlayerId)
                {
                    bool isConnected = connectedPlayerIds.Contains(player.PlayerId);
                    bool isFriend = subscriber.IsFriendOf(player.PlayerId);
                    AddPlayer(player, isConnected, isFriend);
                }
            }
        }

        public IList<Player> AllOnlinePlayers { get; private set; }
        public IList<Player> AllOfflinePlayers { get; private set; }
        public IList<Player> AllOnlineFriends { get; private set; }
        public IList<Player> AllOfflineFriends { get; private set; }

        private void AddPlayer(Player player, bool isConnected, bool isFriend)
        {
            if (isConnected)
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
