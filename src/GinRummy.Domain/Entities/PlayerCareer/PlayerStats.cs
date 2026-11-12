using System;

namespace GinRummy.Domain.Entities.PlayerCareer
{
    public class PlayerStats
    {
        private const int InitialScore = 1000;

        public PlayerStats(int playerAccountId)
        {
            ValidatePlayerAccountId(playerAccountId);

            PlayerAccountId = playerAccountId;
            Wins = 0;
            Losses = 0;
            Score = InitialScore;
        }

        protected PlayerStats()
        {
        }

        public int PlayerAccountId { get; private set; }

        public int Wins { get; private set; }

        public int Losses { get; private set; }

        public int Score { get; private set; }

        public int MatchesPlayed { get; private set; }

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

        private static void ValidatePlayerAccountId(int playerAccountId)
        {
            if (playerAccountId <= 0)
            {
                throw new ArgumentException("Player account id must be positive.", nameof(playerAccountId));
            }
        }
    }
}
