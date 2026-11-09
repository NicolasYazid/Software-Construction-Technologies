using System;

namespace GinRummy.Domain.Entities.PlayerCareer
{
    /// <summary>
    /// The stats of a player together with the username the leaderboard shows next to them. It is
    /// read with a query that joins the stats and the profile of the player, so the stats do not
    /// need to point to the profile.
    /// </summary>
    public class LeaderboardEntry
    {
        /// <summary>
        /// Builds an entry with the stats and the username of the same player.
        /// </summary>
        /// <param name="stats">The player's stats.</param>
        /// <param name="username">The username of the player.</param>
        /// <exception cref="ArgumentNullException">Thrown when the stats or the username are null.</exception>
        public LeaderboardEntry(PlayerStats stats, string username)
        {
            Stats = stats ?? throw new ArgumentNullException(nameof(stats));
            Username = username ?? throw new ArgumentNullException(nameof(username));
        }

        /// <summary>
        /// Gets the player's stats.
        /// </summary>
        public PlayerStats Stats { get; private set; }

        /// <summary>
        /// Gets the username of the player.
        /// </summary>
        public string Username { get; private set; }
    }
}
