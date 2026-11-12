using System;

namespace GinRummy.Domain.Entities.PlayerCareer
{
    public class LeaderboardPosition
    {
        public LeaderboardPosition(int place, PlayerStats stats, string username, Rank rank)
        {
            Place = place;
            Stats = stats ?? throw new ArgumentNullException(nameof(stats));
            Username = username ?? throw new ArgumentNullException(nameof(username));
            Rank = rank;
        }

        public int Place { get; private set; }

        public PlayerStats Stats { get; private set; }

        public string Username { get; private set; }

        public Rank Rank { get; private set; }
    }
}
