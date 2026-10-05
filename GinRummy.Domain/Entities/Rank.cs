using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GinRummy.Domain.Entities
{
    // A rank tier, mapped to the Rank table. A player's rank is the tier whose score range
    // contains the player's score.
    public class Rank
    {
        protected Rank()
        {
        }

        public int RankId { get; private set; }
        public string Name { get; private set; }
        public int MinimumScore { get; private set; }
        public int MaximumScore { get; private set; }
    }
}
