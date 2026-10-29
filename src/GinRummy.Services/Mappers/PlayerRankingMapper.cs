using System.Collections.Generic;

using GinRummy.Contracts.DataContracts;
using GinRummy.Domain.Entities;

namespace GinRummy.Services.Mappers
{
    /// <summary>
    /// Turns the leaderboard positions of the domain into the rows the client shows.
    /// </summary>
    public static class PlayerRankingMapper
    {
        /// <summary>
        /// Maps every leaderboard position to its ranking row, keeping their order.
        /// </summary>
        /// <param name="leaderboardPositions">Positions of the leaderboard, already ordered.</param>
        /// <returns>The ranking rows; empty when there are no positions.</returns>
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
            PlayerRanking ranking = new PlayerRanking();
            ranking.Position = position.Place;
            ranking.Username = position.Stats.Player.Username;
            ranking.RankName = ResolveRankName(position.Rank);
            ranking.Wins = position.Stats.Wins;
            ranking.Losses = position.Stats.Losses;
            ranking.WinRate = position.Stats.WinRate;

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
