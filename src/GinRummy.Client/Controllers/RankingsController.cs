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
    // Orchestrates CU-19 (View Leaderboard) at this activity's scope: reads the ranked stats
    // and resolves each player's rank. It only reads data, so it needs no transaction.
    public class RankingsController
    {
        private const string ServiceUnavailableMessageKey = "Error_SysServiceUnavailable";

        private readonly IRankingDao _rankingDao;
        private IList<Rank> _ranks;

        public RankingsController(IRankingDao rankingDao)
        {
            _rankingDao = rankingDao;
            _ranks = new List<Rank>();
        }

        public string ErrorMessageKey { get; private set; }

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
