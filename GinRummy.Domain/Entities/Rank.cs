using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GinRummy.Domain.Entities
{
    /// <summary>
    /// A rank tier, mapped to the Rank table. A player's rank is the tier whose score
    /// range contains the player's score.
    /// </summary>
    public class Rank
    {
        /// <summary>
        /// Parameterless constructor reserved for Entity Framework's materialization.
        /// </summary>
        protected Rank()
        {
        }

        /// <summary>
        /// Gets the internal identifier.
        /// </summary>
        public int RankId { get; private set; }

        /// <summary>
        /// Gets the display name of the rank, such as "Bronce" or "Oro".
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the lowest score that belongs to this rank.
        /// </summary>
        public int MinimumScore { get; private set; }

        /// <summary>
        /// Gets the highest score that belongs to this rank.
        /// </summary>
        public int MaximumScore { get; private set; }
    }
}
