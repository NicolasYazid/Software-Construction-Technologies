using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

using GinRummy.Client.Models;
using GinRummy.Domain.Daos;
using GinRummy.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace GinRummy.Client.Controllers
{
    // It only reads data, so it needs no transaction.
    public class RankingsController
    {
        private const string ServiceUnavailableMessageKey = "Error_SysServiceUnavailable";
        private const int FirstPosition = 1;

        private readonly IRankingDao _rankingDao;
        private readonly ILogger<RankingsController> _logger;
        private IList<Rank> _ranks;

        public RankingsController(IRankingDao rankingDao, ILogger<RankingsController> logger)
        {
            _rankingDao = rankingDao;
            _logger = logger;
            _ranks = new List<Rank>();
        }

        public string ErrorMessageKey { get; private set; }

        public LeaderboardDto GetGlobalLeaderboard()
        {
            IList<PlayerStats> rankedStats = LoadRankedStats();
            List<RankingEntryDto> entries = new List<RankingEntryDto>();
            int position = FirstPosition;
            foreach (PlayerStats stats in rankedStats)
            {
                entries.Add(ToRankingEntry(stats, position));
                position++;
            }

            LeaderboardDto leaderboard = new LeaderboardDto();
            leaderboard.Entries = entries;

            // The highlighted own row of CU-19 FA-03 needs the signed-in player, and only the server will know who that is.
            leaderboard.OwnEntry = null;

            return leaderboard;
        }

        private IList<PlayerStats> LoadRankedStats()
        {
            IList<PlayerStats> rankedStats;
            ErrorMessageKey = null;
            try
            {
                rankedStats = _rankingDao.GetStatsRankedByScore();
                _ranks = _rankingDao.GetAllRanks();
            }
            catch (DataException ex)
            {
                _logger.LogError(ex, "The database could not be reached to load the global leaderboard.");
                rankedStats = new List<PlayerStats>();
                ErrorMessageKey = ServiceUnavailableMessageKey;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "The database could not be reached to load the global leaderboard.");
                rankedStats = new List<PlayerStats>();
                ErrorMessageKey = ServiceUnavailableMessageKey;
            }

            return rankedStats;
        }

        private RankingEntryDto ToRankingEntry(PlayerStats stats, int position)
        {
            RankingEntryDto entry = new RankingEntryDto();
            entry.Position = position;
            entry.Username = stats.Player.Username;
            entry.Wins = stats.Wins;
            entry.Losses = stats.Losses;
            entry.WinRate = stats.WinRate;
            entry.RankName = ResolveRankName(stats.Score);
            entry.IsOwnEntry = false;

            return entry;
        }

        private string ResolveRankName(int score)
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
