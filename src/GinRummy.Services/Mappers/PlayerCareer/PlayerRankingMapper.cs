using System.Collections.Generic;

using GinRummy.Contracts.DataContracts.PlayerCareer;
using GinRummy.Domain.Entities.PlayerCareer;

namespace GinRummy.Services.Mappers.PlayerCareer
{
    public static class PlayerRankingMapper
    {
        public static List<PlayerRanking> MapToPlayerRankings(IReadOnlyList<LeaderboardPosition> leaderboardPositions)
        {
            List<PlayerRanking> rankings = new List<PlayerRanking>();
            foreach (LeaderboardPosition position in leaderboardPositions)
            {
                rankings.Add(MapToPlayerRanking(position));
            }

            return rankings;
        }

        private static PlayerRanking MapToPlayerRanking(LeaderboardPosition position)
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
