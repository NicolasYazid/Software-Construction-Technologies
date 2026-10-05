using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GinRummy.Domain.Entities;

namespace GinRummy.Domain.Services
{
    /// <summary>
    /// Resolves which rank a score belongs to, applying the rule that a rank is the tier
    /// whose score range contains the score.
    /// </summary>
    public class RankResolver
    {
        /// <summary>
        /// Finds the rank whose score range contains the given score.
        /// </summary>
        /// <param name="score">The player's score.</param>
        /// <param name="ranks">The rank catalog to search.</param>
        /// <returns>The matching rank, or null when none contains the score.</returns>
        public Rank ResolveRank(int score, IList<Rank> ranks)
        {
            Rank matchingRank = ranks.FirstOrDefault(rank => (score >= rank.MinimumScore) && (score <= rank.MaximumScore));

            return matchingRank;
        }
    }
}
