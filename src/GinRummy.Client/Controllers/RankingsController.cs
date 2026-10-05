using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GinRummy.Domain.Daos;
using GinRummy.Domain.Entities;

namespace GinRummy.Client.Controllers
{
    /// <summary>
    /// Orchestrates CU-19 (View Leaderboard) at this activity's scope: reads the ranked
    /// stats and resolves each player's rank. It only reads data, so it needs no transaction.
    /// </summary>
    public class RankingsController
    {
        private const string ServiceUnavailableMessageKey = "Error_SysServiceUnavailable";

        private readonly IRankingDao _rankingDao;
        private IList<Rank> _ranks;

        /// <summary>
        /// Builds the controller with the ranking DAO the composition root assembled.
        /// </summary>
        /// <param name="rankingDao">DAO used to read stats and ranks.</param>
        public RankingsController(IRankingDao rankingDao)
        {
            _rankingDao = rankingDao;
            _ranks = new List<Rank>();
        }

        /// <summary>
        /// Gets the localization key of the error of the last load, or null when it succeeded.
        /// </summary>
        public string ErrorMessageKey { get; private set; }

        /// <summary>
        /// Gets every player's stats ordered from the highest score to the lowest, and loads
        /// the rank catalog used to name each player's rank. When the database cannot be
        /// reached, returns an empty list and sets <see cref="ErrorMessageKey"/>.
        /// </summary>
        /// <returns>The ranked stats, each with its player loaded, or an empty list.</returns>
        public IList<PlayerStats> GetGlobalRanking()
        {
            IList<PlayerStats> rankedStats;
            ErrorMessageKey = null;
            try
            {
                rankedStats = _rankingDao.GetStatsRankedByScore();
                _ranks = _rankingDao.GetAllRanks();
            }
            catch (DataException)
            {
                rankedStats = new List<PlayerStats>();
                ErrorMessageKey = ServiceUnavailableMessageKey;
            }
            catch (SqlException)
            {
                rankedStats = new List<PlayerStats>();
                ErrorMessageKey = ServiceUnavailableMessageKey;
            }

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
