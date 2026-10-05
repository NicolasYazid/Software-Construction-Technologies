using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GinRummy.Data.EntityFramework.Persistence;
using GinRummy.Domain.Daos;
using GinRummy.Domain.Entities;

namespace GinRummy.Data.EntityFramework.Daos
{
    // Fulfils IRankingDao using Entity Framework against GinRummy_Dev.
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
                // Include loads each player together with its stats in one query, so reading
                // the username later does not trigger a separate query per row.
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
