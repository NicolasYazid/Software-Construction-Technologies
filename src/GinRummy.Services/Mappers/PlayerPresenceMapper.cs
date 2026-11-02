using System.Collections.Generic;

using GinRummy.Application.Presence;
using GinRummy.Contracts.DataContracts;
using GinRummy.Domain.Entities.Players;

namespace GinRummy.Services.Mappers
{
    /// <summary>
    /// Turns the players of the domain and their lists into the data the client receives.
    /// </summary>
    public static class PlayerPresenceMapper
    {
        /// <summary>
        /// Maps the four lists of players to the report the client receives. Every player of a list is marked as
        /// connected or not by the list it belongs to, and as a friend when it also belongs to the matching list of friends.
        /// </summary>
        /// <param name="lists">Lists of the players, split into connected and not connected.</param>
        /// <returns>The report; its lists are empty when nobody belongs to them.</returns>
        public static PlayerPresenceReport MapToPlayerPresenceReport(PlayerPresenceLists lists)
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

        /// <summary>
        /// Maps one player to the data the client receives.
        /// </summary>
        /// <param name="player">Player to map.</param>
        /// <param name="status">Whether the player is connected.</param>
        /// <param name="isFriend">Whether the player is a friend of whoever receives the data.</param>
        /// <returns>The player as the client receives it.</returns>
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
                // The lists of friends hold the very same player objects as the lists of players, so Contains finds them.
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
