using System;

using GinRummy.Domain.Entities.Authentication;

namespace GinRummy.Domain.Entities.Gameplay
{
    /// <summary>
    /// The result of a finished match from the point of view of one registered player, mapped
    /// to the GameHistory table. Protects its own invariants: the opponent is another player
    /// and the points earned are not negative.
    /// </summary>
    public class GameHistory
    {
        private const int MinimumScore = 0;

        /// <summary>
        /// Creates a valid new history entry.
        /// </summary>
        /// <param name="playerAccountId">Identifier of the player the entry belongs to; must be positive.</param>
        /// <param name="opponentId">Identifier of the opponent; positive and different from the player.</param>
        /// <param name="hasWon">True when the player won the match.</param>
        /// <param name="scoreEarned">Points earned in the match; not negative.</param>
        /// <param name="playedAt">Moment the match was played, in UTC.</param>
        public GameHistory(int playerAccountId, int opponentId, bool hasWon, int scoreEarned, DateTime playedAt)
        {
            ValidatePlayers(playerAccountId, opponentId);
            ValidateScore(scoreEarned);

            PlayerAccountId = playerAccountId;
            OpponentId = opponentId;
            HasWon = hasWon;
            ScoreEarned = scoreEarned;
            PlayedAt = playedAt;
        }

        /// <summary>
        /// Parameterless constructor reserved for Entity Framework's materialization; not
        /// for application code, which must use the validating constructor.
        /// </summary>
        protected GameHistory()
        {
        }

        /// <summary>
        /// Gets the internal, autonumeric identifier.
        /// </summary>
        public int MatchId { get; private set; }

        /// <summary>
        /// Gets the identifier of the player the entry belongs to.
        /// </summary>
        public int PlayerAccountId { get; private set; }

        /// <summary>
        /// Gets the identifier of the opponent.
        /// </summary>
        public int OpponentId { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the player won. The column keeps the name game_result.
        /// </summary>
        public bool HasWon { get; private set; }

        /// <summary>
        /// Gets the points the player earned in the match.
        /// </summary>
        public int ScoreEarned { get; private set; }

        /// <summary>
        /// Gets the moment the match was played, in UTC.
        /// </summary>
        public DateTime PlayedAt { get; private set; }

        /// <summary>
        /// Gets the account of the player the entry belongs to.
        /// </summary>
        public virtual PlayerAccount PlayerAccount { get; private set; }

        /// <summary>
        /// Gets the account of the opponent.
        /// </summary>
        public virtual PlayerAccount Opponent { get; private set; }

        // Both sides are real players, and nobody plays a match against themselves.
        private static void ValidatePlayers(int playerAccountId, int opponentId)
        {
            if ((playerAccountId <= 0) || (opponentId <= 0))
            {
                throw new ArgumentException("Player account ids must be positive.");
            }

            if (playerAccountId == opponentId)
            {
                throw new ArgumentException("The opponent must be another player.", nameof(opponentId));
            }
        }

        // Points are only earned, so a negative value would be a calculation error.
        private static void ValidateScore(int scoreEarned)
        {
            if (scoreEarned < MinimumScore)
            {
                throw new ArgumentOutOfRangeException(nameof(scoreEarned), "The score cannot be negative.");
            }
        }
    }
}
