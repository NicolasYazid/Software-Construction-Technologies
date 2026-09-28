using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GinRummy.Domain.Entities;

namespace GinRummy.Domain.Repositories
{
    /// <summary>
    /// Contract the game's logic uses to build the leaderboard, without knowing how or
    /// where the data is stored.
    /// </summary>
    public interface IRankingRepository
    {
        /// <summary>
        /// Gets every player's stats ordered from the highest score to the lowest, each
        /// with its player loaded.
        /// </summary>
        /// <returns>The ranked stats.</returns>
        IList<PlayerStats> GetStatsRankedByScore();

        /// <summary>
        /// Gets the full rank catalog, used to resolve which rank a score belongs to.
        /// </summary>
        /// <returns>The ranks.</returns>
        IList<Rank> GetAllRanks();
    }
}
