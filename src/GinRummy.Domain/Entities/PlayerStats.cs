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
        private const int InitialScore = 1000;

        /// <summary>
        /// Creates the stats every new account starts with: no wins, no losses and the
        /// initial score.
        /// </summary>
        /// <param name="playerId">Identifier of the account the stats belong to; must be positive.</param>
        public PlayerStats(int playerId)
        {
            ValidatePlayerId(playerId);

            PlayerId = playerId;
            Wins = 0;
            Losses = 0;
            Score = InitialScore;
        }

        /// <summary>
        /// Parameterless constructor reserved for Entity Framework's materialization; not
        /// for application code, which must use the validating constructor.
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

        /// <summary>
        /// Gets the share of matches won, from zero to one. It is zero when no match has been
        /// played, so the division is never by zero.
        /// </summary>
        public double WinRate
        {
            get
            {
                double winRate = 0d;
                if (MatchesPlayed > 0)
                {
                    winRate = (double)Wins / MatchesPlayed;
                }

                return winRate;
            }
        }

        // The stats of an account are keyed by the account, which must already exist.
        private static void ValidatePlayerId(int playerId)
        {
            if (playerId <= 0)
            {
                throw new ArgumentException("Player id must be positive.", nameof(playerId));
            }
        }
    }
}
