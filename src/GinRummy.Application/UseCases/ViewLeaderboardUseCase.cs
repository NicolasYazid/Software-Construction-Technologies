using System;
using System.Collections.Generic;
using System.Linq;

using GinRummy.Application.Ports;
using GinRummy.Domain.Entities;
using GinRummy.Domain.Ports.Repositories;
using GinRummy.Domain.Services;

namespace GinRummy.Application.UseCases
{
    /// <summary>
    /// The View Leaderboard use case: reads the players' stats, orders them by the rules of
    /// CU-19, and pairs each one with the rank its score falls into.
    /// </summary>
    public class ViewLeaderboardUseCase : ILeaderboardPort
    {
        private const int FirstPlace = 1;

        private readonly IRankingRepository _rankingRepository;
        private readonly RankResolver _rankResolver;

        /// <summary>
        /// Builds the use case with the ranking repository and the rank resolver the
        /// composition root assembled.
        /// </summary>
        /// <param name="rankingRepository">Repository used to read stats and ranks.</param>
        /// <param name="rankResolver">Domain rule that resolves a score into its rank.</param>
        /// <exception cref="ArgumentNullException">Thrown when a dependency is null.</exception>
        public ViewLeaderboardUseCase(IRankingRepository rankingRepository, RankResolver rankResolver)
        {
            _rankingRepository = rankingRepository ?? throw new ArgumentNullException(nameof(rankingRepository));
            _rankResolver = rankResolver ?? throw new ArgumentNullException(nameof(rankResolver));
        }

        /// <summary>
        /// Gets the ordered leaderboard, each position with its place and resolved rank.
        /// </summary>
        /// <returns>The leaderboard positions; empty when there are no players.</returns>
        public IReadOnlyList<LeaderboardPosition> GetLeaderboard()
        {
            IList<PlayerStats> stats = _rankingRepository.GetStatsRankedByScore();
            IList<Rank> ranks = _rankingRepository.GetAllRanks();
            IReadOnlyList<LeaderboardPosition> leaderboard = BuildLeaderboard(stats, ranks);

            return leaderboard;
        }

        private IReadOnlyList<LeaderboardPosition> BuildLeaderboard(IList<PlayerStats> stats, IList<Rank> ranks)
        {
            List<LeaderboardPosition> leaderboard = new List<LeaderboardPosition>();
            IList<PlayerStats> orderedStats = OrderByLeaderboardRules(stats);
            int place = FirstPlace;
            foreach (PlayerStats playerStats in orderedStats)
            {
                Rank rank = _rankResolver.ResolveRank(playerStats.Score, ranks);
                leaderboard.Add(new LeaderboardPosition(place, playerStats, rank));
                place++;
            }

            return leaderboard;
        }

        // The leaderboard orders by wins first (CU-19), and score, losses and username only break ties.
        // The repository's own order by score is intentionally overridden here.
        private IList<PlayerStats> OrderByLeaderboardRules(IList<PlayerStats> stats)
        {
            IList<PlayerStats> orderedStats = stats
                .OrderByDescending(playerStats => playerStats.Wins)
                .ThenByDescending(playerStats => playerStats.Score)
                .ThenBy(playerStats => playerStats.Losses)
                .ThenBy(playerStats => playerStats.Player.Username)
                .ToList();

            return orderedStats;
        }
    }
}
