using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GinRummy.Domain.Entities
{
    // A player's accumulated performance, mapped to the PlayerStats table. It always belongs to
    // exactly one player.
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

        // A player with no matches gets zero, so the division is never by zero.
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
