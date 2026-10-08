namespace GinRummy.Domain.Entities
{
    public class LeaderboardPosition
    {
        public LeaderboardPosition(int place, PlayerStats stats, Rank rank)
        {
            Place = place;
            Stats = stats;
            Rank = rank;
        }

        public int Place { get; private set; }
        public PlayerStats Stats { get; private set; }
        public Rank Rank { get; private set; }
    }
}
