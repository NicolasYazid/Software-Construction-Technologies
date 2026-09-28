using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GinRummy.Data.EntityFramework.Persistence;
using GinRummy.Domain.Entities;
using GinRummy.Domain.Repositories;

namespace GinRummy.Data.EntityFramework.Repositories
{
    /// <summary>
    /// Fulfils IRankingRepository using Entity Framework against GinRummy_Dev.
    /// </summary>
    public class RankingRepository : IRankingRepository
    {
        private readonly string _connectionStringName;

        /// <summary>
        /// Builds the repository against the given connection string entry.
        /// </summary>
        /// <param name="connectionStringName">Name of the entry in App.config.</param>
        public RankingRepository(string connectionStringName)
        {
            _connectionStringName = connectionStringName;
        }

        /// <summary>
        /// Gets every player's stats ordered from the highest score to the lowest, each
        /// with its player loaded.
        /// </summary>
        /// <returns>The ranked stats.</returns>
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

        /// <summary>
        /// Gets the full rank catalog, used to resolve which rank a score belongs to.
        /// </summary>
        /// <returns>The ranks.</returns>
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
