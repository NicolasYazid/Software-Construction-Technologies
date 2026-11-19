using System;
using System.Collections.Generic;

using GinRummy.Contracts.DataContracts.PlayerCareer;
using GinRummy.Domain.Entities.PlayerCareer;
using ContractLeaderboardType = GinRummy.Contracts.DataContracts.PlayerCareer.LeaderboardType;
using DomainLeaderboardType = GinRummy.Domain.Entities.PlayerCareer.LeaderboardType;

namespace GinRummy.Services.Mappers.PlayerCareer
{
    public static class PlayerRankingMapper
    {
        public static List<PlayerRanking> MapToPlayerRankings(IReadOnlyList<LeaderboardPlayerPosition> leaderboardPositions)
        {
            List<PlayerRanking> rankings = new List<PlayerRanking>();
            foreach (LeaderboardPlayerPosition position in leaderboardPositions)
            {
                rankings.Add(MapToPlayerRanking(position));
            }

            return rankings;
        }

        public static DomainLeaderboardType MapToDomainLeaderboardType(ContractLeaderboardType leaderboardType)
        {
            return (DomainLeaderboardType)Enum.Parse(typeof(DomainLeaderboardType), leaderboardType.ToString());
        }

        private static PlayerRanking MapToPlayerRanking(LeaderboardPlayerPosition position)
        {
            PlayerRanking ranking = new PlayerRanking
            {
                Position = position.Place,
                Username = position.Username,
                RankName = ResolveRankName(position.Rank),
                Wins = position.Stats.Wins,
                Losses = position.Stats.Losses,
                WinRate = position.Stats.WinRate
            };

            return ranking;
        }

        private static string ResolveRankName(Rank rank)
        {
            string rankName = string.Empty;
            if (rank != null)
            {
                rankName = rank.Name;
            }

            return rankName;
        }
    }
}
