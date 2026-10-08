namespace GinRummy.Domain.Entities
{
    public class PlayerStats
    {
        protected PlayerStats()
        {
        }

        public int PlayerId { get; private set; }
        public int Wins { get; private set; }
        public int Losses { get; private set; }
        public int Score { get; private set; }
        public int MatchesPlayed { get; private set; }
        public virtual Player Player { get; private set; }

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
    }
}
