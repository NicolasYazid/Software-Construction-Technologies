using System;
using System.Collections.Generic;
using System.Linq;

using GinRummy.Application.Ports;
using GinRummy.Domain.Entities.PlayerRanking;
using GinRummy.Domain.Entities.Players;
using GinRummy.Domain.Ports.Repositories;
using GinRummy.Domain.Services;

namespace GinRummy.Application.UseCases
{
    public class ViewLeaderboardUseCase : ILeaderboardPort
    {
        private const int FirstPlace = 1;

        private readonly IRankingRepository _rankingRepository;
        private readonly RankResolver _rankResolver;

        public ViewLeaderboardUseCase(IRankingRepository rankingRepository, RankResolver rankResolver)
        {
            _rankingRepository = rankingRepository ?? throw new ArgumentNullException(nameof(rankingRepository));
            _rankResolver = rankResolver ?? throw new ArgumentNullException(nameof(rankResolver));
        }

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
