using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using GinRummy.Domain.Entities.PlayerCareer;
using GinRummy.Domain.Ports.PlayerCareer;
using GinRummy.Persistence.Persistence;

namespace GinRummy.Persistence.Repositories
{
    /// <summary>
    /// Fulfils IRankingRepository using Entity Framework against GinRummy_Dev. By default,
    /// opens a short-lived context per operation; optionally reuses a context someone else
    /// already opened, so it can be read inside a unit of work.
    /// </summary>
    public class RankingRepository : IRankingRepository
    {
        private readonly string _connectionStringName;
        private readonly GinRummyContext _sharedContext;

        /// <summary>
        /// Builds the repository against the given connection string entry.
        /// </summary>
        /// <param name="connectionStringName">Name of the entry in App.config.</param>
        /// <exception cref="ArgumentNullException">Thrown when the name is null.</exception>
        public RankingRepository(string connectionStringName)
        {
            _connectionStringName = connectionStringName
                ?? throw new ArgumentNullException(nameof(connectionStringName));
        }

        /// <summary>
        /// Builds the repository so that every operation runs on a context someone else
        /// already opened. The caller stays responsible for disposing that context.
        /// </summary>
        /// <param name="sharedContext">Context to reuse instead of opening a new one.</param>
        /// <exception cref="ArgumentNullException">Thrown when the context is null.</exception>
        public RankingRepository(GinRummyContext sharedContext)
        {
            _sharedContext = sharedContext ?? throw new ArgumentNullException(nameof(sharedContext));
        }

        /// <summary>
        /// Gets every player's stats ordered from the highest score to the lowest, each
        /// with its player loaded.
        /// </summary>
        /// <returns>The ranked stats.</returns>
        public IList<PlayerStats> GetStatsRankedByScore()
        {
            IList<PlayerStats> rankedStats;
            if (_sharedContext != null)
            {
                rankedStats = ReadStatsRankedByScore(_sharedContext);
            }
            else
            {
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    rankedStats = ReadStatsRankedByScore(context);
                }
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
            if (_sharedContext != null)
            {
                ranks = _sharedContext.Ranks.ToList();
            }
            else
            {
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    ranks = context.Ranks.ToList();
                }
            }

            return ranks;
        }

        // Each player is loaded together with its stats in one query.
        // Reading the username later therefore does not trigger a separate query per row.
        private static IList<PlayerStats> ReadStatsRankedByScore(GinRummyContext context)
        {
            IList<PlayerStats> rankedStats = context.PlayerStats
                .Include(stats => stats.PlayerProfile)
                .OrderByDescending(stats => stats.Score)
                .ToList();

            return rankedStats;
        }
    }
}
