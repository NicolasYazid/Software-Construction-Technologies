namespace GinRummy.Domain.Entities
{
    // One already-ordered leaderboard row: the place it holds, a player's stats, and the rank the score falls into.
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
