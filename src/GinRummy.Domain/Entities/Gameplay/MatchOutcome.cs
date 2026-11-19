using System;

namespace GinRummy.Domain.Entities.Gameplay
{
    public class MatchOutcome
    {
        public MatchOutcome(bool hasWon, int scoreEarned, DateTime playedAt)
        {
            HasWon = hasWon;
            ScoreEarned = scoreEarned;
            PlayedAt = playedAt;
        }

        public bool HasWon { get; private set; }

        public int ScoreEarned { get; private set; }

        public DateTime PlayedAt { get; private set; }
    }
}
