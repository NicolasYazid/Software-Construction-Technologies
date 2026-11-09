using System;

namespace GinRummy.Domain.Entities.PlayerCareer
{
    /// <summary>
    /// One already-ordered leaderboard row: the place it holds, a player's stats and username,
    /// and the rank the score falls into.
    /// </summary>
    public class LeaderboardPosition
    {
        /// <summary>
        /// Builds a position with its place, stats, username and resolved rank.
        /// </summary>
        /// <param name="place">The place in the ordering, starting at one.</param>
        /// <param name="stats">The player's stats.</param>
        /// <param name="username">The username of the player.</param>
        /// <param name="rank">The rank the score falls into, or null when none matches.</param>
        /// <exception cref="ArgumentNullException">Thrown when the stats or the username are null.</exception>
        public LeaderboardPosition(int place, PlayerStats stats, string username, Rank rank)
        {
            Place = place;
            Stats = stats ?? throw new ArgumentNullException(nameof(stats));
            Username = username ?? throw new ArgumentNullException(nameof(username));
            Rank = rank;
        }

        /// <summary>
        /// Gets the place in the ordering, starting at one.
        /// </summary>
        public int Place { get; private set; }

        /// <summary>
        /// Gets the player's stats.
        /// </summary>
        public PlayerStats Stats { get; private set; }

        /// <summary>
        /// Gets the username of the player.
        /// </summary>
        public string Username { get; private set; }

        /// <summary>
        /// Gets the rank the score falls into, or null when none matches.
        /// </summary>
        public Rank Rank { get; private set; }
    }
}
