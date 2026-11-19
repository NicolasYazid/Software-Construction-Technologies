using System;

using GinRummy.Domain.Entities.Authentication;

namespace GinRummy.Domain.Entities.Gameplay
{
    public class GameHistory
    {
        private const int MinimumScore = 0;

        public GameHistory(int playerAccountId, int opponentId, MatchOutcome outcome)
        {
            if (outcome == null)
            {
                throw new ArgumentNullException(nameof(outcome));
            }

            ValidatePlayers(playerAccountId, opponentId);
            ValidateScore(outcome.ScoreEarned);

            PlayerAccountId = playerAccountId;
            OpponentId = opponentId;
            HasWon = outcome.HasWon;
            ScoreEarned = outcome.ScoreEarned;
            PlayedAt = outcome.PlayedAt;
        }

        protected GameHistory()
        {
        }

        public int MatchId { get; private set; }

        public int PlayerAccountId { get; private set; }

        public int OpponentId { get; private set; }

        public bool HasWon { get; private set; }

        public int ScoreEarned { get; private set; }

        public DateTime PlayedAt { get; private set; }

        public virtual PlayerAccount PlayerAccount { get; private set; }

        public virtual PlayerAccount Opponent { get; private set; }

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

        private static void ValidateScore(int scoreEarned)
        {
            if (scoreEarned < MinimumScore)
            {
                throw new ArgumentOutOfRangeException(nameof(scoreEarned), "The score cannot be negative.");
            }
        }
    }
}
