using System.Collections.Generic;

using GinRummy.Domain.Entities.PlayerCareer;

namespace GinRummy.Domain.Ports.PlayerCareer
{
    /// <summary>
    /// Contract the game's logic uses to build the leaderboard, without knowing how or
    /// where the data is stored.
    /// </summary>
    public interface IRankingRepository
    {
        /// <summary>
        /// Gets every player's stats ordered from the highest score to the lowest.
        /// </summary>
        /// <returns>The ranked stats.</returns>
        IList<PlayerStats> GetStatsRankedByScore();

        /// <summary>
        /// Gets the stats and the username of every player, ordered from the highest score to the
        /// lowest. Only the stats and the username are read, never the rest of the profile.
        /// </summary>
        /// <returns>The ranked entries.</returns>
        IList<LeaderboardEntry> GetLeaderboardEntriesRankedByScore();

        /// <summary>
        /// Gets the full rank catalog, used to resolve which rank a score belongs to.
        /// </summary>
        /// <returns>The ranks.</returns>
        IList<Rank> GetAllRanks();
    }
}
