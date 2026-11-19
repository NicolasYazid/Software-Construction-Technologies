using System;

namespace GinRummy.Domain.Entities.PlayerCareer
{
    public class LeaderboardPlayerPosition
    {
        public LeaderboardPlayerPosition(int place, PlayerStats stats, RankedUsername rankedUsername)
        {
            if (rankedUsername == null)
            {
                throw new ArgumentNullException(nameof(rankedUsername));
            }

            Place = place;
            Stats = stats ?? throw new ArgumentNullException(nameof(stats));
            Username = rankedUsername.Username ?? throw new ArgumentNullException(nameof(rankedUsername));
            Rank = rankedUsername.Rank;
        }

        public int Place { get; private set; }

        public PlayerStats Stats { get; private set; }

        public string Username { get; private set; }

        public Rank Rank { get; private set; }
    }
}
