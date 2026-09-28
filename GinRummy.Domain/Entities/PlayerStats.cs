using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GinRummy.Domain.Entities
{
    /// <summary>
    /// A player's accumulated performance, mapped to the PlayerStats table. It always
    /// belongs to exactly one player.
    /// </summary>
    public class PlayerStats
    {
        /// <summary>
        /// Parameterless constructor reserved for Entity Framework's materialization.
        /// </summary>
        protected PlayerStats()
        {
        }

        /// <summary>
        /// Gets the identifier of the player these stats belong to.
        /// </summary>
        public int PlayerId { get; private set; }

        /// <summary>
        /// Gets how many matches the player has won.
        /// </summary>
        public int Wins { get; private set; }

        /// <summary>
        /// Gets how many matches the player has lost.
        /// </summary>
        public int Losses { get; private set; }

        /// <summary>
        /// Gets the player's score, used to rank the leaderboard.
        /// </summary>
        public int Score { get; private set; }

        /// <summary>
        /// Gets how many matches the player has played, computed by SQL Server.
        /// </summary>
        public int MatchesPlayed { get; private set; }

        /// <summary>
        /// Gets the player these stats belong to, loaded from the relationship between the
        /// two tables.
        /// </summary>
        public virtual Player Player { get; private set; }
    }
}
