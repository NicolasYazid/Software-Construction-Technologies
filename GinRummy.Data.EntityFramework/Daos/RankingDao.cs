using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using GinRummy.Data.EntityFramework.Persistence;
using GinRummy.Domain.Daos;
using GinRummy.Domain.Entities;

namespace GinRummy.Data.EntityFramework.Daos
{
    public class RankingDao : IRankingDao
    {
        private readonly string _connectionStringName;

        public RankingDao(string connectionStringName)
        {
            _connectionStringName = connectionStringName;
        }

        public IList<PlayerStats> GetStatsRankedByScore()
        {
            IList<PlayerStats> rankedStats;
            using (GinRummyContext context = new GinRummyContext(_connectionStringName))
            {
                // The player is loaded eagerly because the context is disposed of before the leaderboard reads each username.
                rankedStats = context.PlayerStats
                    .Include(stats => stats.Player)
                    .OrderByDescending(stats => stats.Score)
                    .ToList();
            }

            return rankedStats;
        }

        public IList<Rank> GetAllRanks()
        {
            IList<Rank> ranks;
            using (GinRummyContext context = new GinRummyContext(_connectionStringName))
            {
                ranks = context.Ranks.ToList();
            }

            return ranks;
        }
    }
}
