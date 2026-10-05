using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GinRummy.Domain.Entities
{
    /// <summary>
    /// One already-ordered leaderboard row: the place it holds, a player's stats, and the
    /// rank the score falls into.
    /// </summary>
    public class LeaderboardPosition
    {
        /// <summary>
        /// Builds a position with its place, stats and resolved rank.
        /// </summary>
        /// <param name="place">The place in the ordering, starting at one.</param>
        /// <param name="stats">The player's stats.</param>
        /// <param name="rank">The rank the score falls into, or null when none matches.</param>
        public LeaderboardPosition(int place, PlayerStats stats, Rank rank)
        {
            Place = place;
            Stats = stats;
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
        /// Gets the rank the score falls into, or null when none matches.
        /// </summary>
        public Rank Rank { get; private set; }
    }
}
