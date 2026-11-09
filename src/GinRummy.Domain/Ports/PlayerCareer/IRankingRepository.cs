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
        /// Gets the username of every player by the identifier of its account. Only the identifier and
        /// the username are read, never the rest of the profile or of the account.
        /// </summary>
        /// <returns>The usernames by player account identifier.</returns>
        IDictionary<int, string> GetUsernamesByPlayerAccountId();

        /// <summary>
        /// Gets the full rank catalog, used to resolve which rank a score belongs to.
        /// </summary>
        /// <returns>The ranks.</returns>
        IList<Rank> GetAllRanks();
    }
}
