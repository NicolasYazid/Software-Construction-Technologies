using System;
using System.Collections.Generic;
using System.Linq;

using GinRummy.Application.Ports;
using GinRummy.Domain.Entities.PlayerCareer;
using GinRummy.Domain.Ports.PlayerCareer;

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
            IList<LeaderboardEntry> entries = _rankingRepository.GetLeaderboardEntriesRankedByScore();
            IList<Rank> ranks = _rankingRepository.GetAllRanks();
            IReadOnlyList<LeaderboardPosition> leaderboard = BuildLeaderboard(entries, ranks);

            return leaderboard;
        }

        private IReadOnlyList<LeaderboardPosition> BuildLeaderboard(IList<LeaderboardEntry> entries, IList<Rank> ranks)
        {
            List<LeaderboardPosition> leaderboard = new List<LeaderboardPosition>();
            IList<LeaderboardEntry> orderedEntries = OrderByLeaderboardRules(entries);
            int place = FirstPlace;
            foreach (LeaderboardEntry entry in orderedEntries)
            {
                Rank rank = _rankResolver.ResolveRank(entry.Stats.Score, ranks);
                leaderboard.Add(new LeaderboardPosition(place, entry.Stats, entry.Username, rank));
                place++;
            }

            return leaderboard;
        }

        private IList<LeaderboardEntry> OrderByLeaderboardRules(IList<LeaderboardEntry> entries)
        {
            IList<LeaderboardEntry> orderedEntries = entries
                .OrderByDescending(entry => entry.Stats.Wins)
                .ThenByDescending(entry => entry.Stats.Score)
                .ThenBy(entry => entry.Stats.Losses)
                .ThenBy(entry => entry.Username)
                .ToList();

            return orderedEntries;
        }
    }
}
