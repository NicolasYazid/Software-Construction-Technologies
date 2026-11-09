using System.Collections.Generic;

using GinRummy.Application.Presence;
using GinRummy.Contracts.DataContracts;
using GinRummy.Domain.Entities.PlayerPresence;

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

        public static PlayerPresence MapToPlayerPresence(RankedPlayer player, PlayerPresenceStatus status, bool isFriend)
        {
            PlayerPresence presence = new PlayerPresence
            {
                Username = player.DisplayName,
                PublicTag = player.PublicTag,
                Status = MapToPresenceStatus(status),
                IsFriend = isFriend,
                IsGuest = player.IsGuest,
                GuestSessionId = player.AccessTokenOwner.GuestSessionId,
                RankName = PlayerProfileMapper.ResolveRankName(player.Rank),
                Score = player.Score
            };

            return presence;
        }

        private static List<PlayerPresence> MapToPlayerPresences(
            IList<RankedPlayer> players,
            PlayerPresenceStatus status,
            IList<RankedPlayer> friends)
        {
            List<PlayerPresence> presences = new List<PlayerPresence>();
            foreach (RankedPlayer player in players)
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
