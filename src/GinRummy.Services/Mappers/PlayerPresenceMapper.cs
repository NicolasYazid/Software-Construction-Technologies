using System.Collections.Generic;

using GinRummy.Application.Presence;
using GinRummy.Contracts.DataContracts;
using GinRummy.Domain.Entities.Players;

namespace GinRummy.Services.Mappers
{
    public static class PlayerPresenceMapper
    {
        public static PlayerPresenceReport MapToPlayerPresenceReport(PlayersByStatus lists)
        {
            PlayerPresenceReport report = new PlayerPresenceReport
            {
                AllOnlinePlayers = MapToPlayerPresences(lists.AllOnlinePlayers, PlayerPresenceStatus.Online, lists.AllOnlineFriends),
                AllOfflinePlayers = MapToPlayerPresences(lists.AllOfflinePlayers, PlayerPresenceStatus.Offline, lists.AllOfflineFriends),
                AllOnlineFriends = MapToPlayerPresences(lists.AllOnlineFriends, PlayerPresenceStatus.Online, lists.AllOnlineFriends),
                AllOfflineFriends = MapToPlayerPresences(lists.AllOfflineFriends, PlayerPresenceStatus.Offline, lists.AllOfflineFriends)
            };

            return report;
        }

        public static PlayerPresence MapToPlayerPresence(Player player, PlayerPresenceStatus status, bool isFriend)
        {
            PlayerPresence presence = new PlayerPresence
            {
                Username = player.Username,
                PublicTag = player.PublicTag,
                Status = MapToPresenceStatus(status),
                IsFriend = isFriend
            };

            return presence;
        }

        private static List<PlayerPresence> MapToPlayerPresences(IList<Player> players, PlayerPresenceStatus status, IList<Player> friends)
        {
            List<PlayerPresence> presences = new List<PlayerPresence>();
            foreach (Player player in players)
            {
                bool isFriend = friends.Contains(player);
                presences.Add(MapToPlayerPresence(player, status, isFriend));
            }

            return presences;
        }

        private static PresenceStatus MapToPresenceStatus(PlayerPresenceStatus status)
        {
            PresenceStatus mappedStatus = PresenceStatus.Offline;
            if (status == PlayerPresenceStatus.Online)
            {
                mappedStatus = PresenceStatus.Online;
            }

            return mappedStatus;
        }
    }
}
