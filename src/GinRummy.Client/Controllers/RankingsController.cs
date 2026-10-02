using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GinRummy.Domain.Entities;
using GinRummy.Domain.Repositories;

namespace GinRummy.Client.Controllers
{
    /// <summary>
    /// Orchestrates CU-19 (View Leaderboard) at this activity's scope: reads the ranked
    /// stats and resolves each player's rank. It only reads data, so it needs no transaction.
    /// </summary>
    public class RankingsController
    {
        private readonly IRankingRepository _rankingRepository;
        private IList<Rank> _ranks;

        /// <summary>
        /// Builds the controller with the ranking repository the composition root assembled.
        /// </summary>
        /// <param name="rankingRepository">Repository used to read stats and ranks.</param>
        public RankingsController(IRankingRepository rankingRepository)
        {
            _rankingRepository = rankingRepository;
            _ranks = new List<Rank>();
        }

        /// <summary>
        /// Gets every player's stats ordered from the highest score to the lowest, and loads
        /// the rank catalog used to name each player's rank.
        /// </summary>
        /// <returns>The ranked stats, each with its player loaded.</returns>
        public IList<PlayerStats> GetGlobalRanking()
        {
            IList<PlayerStats> rankedStats = _rankingRepository.GetStatsRankedByScore();
            _ranks = _rankingRepository.GetAllRanks();

            return rankedStats;
        }

        /// <summary>
        /// Resolves the rank name for a score against the catalog loaded by
        /// <see cref="GetGlobalRanking"/>, or an empty string when none matches.
        /// </summary>
        /// <param name="score">Score to place within the rank catalog.</param>
        /// <returns>The name of the matching rank, or an empty string.</returns>
        public string ResolveRankName(int score)
        {
            string rankName = string.Empty;
            Rank matchingRank = _ranks.FirstOrDefault(
                rank => (score >= rank.MinimumScore) && (score <= rank.MaximumScore));
            if (matchingRank != null)
            {
                rankName = matchingRank.Name;
            }

            return rankName;
        }
    }
}
