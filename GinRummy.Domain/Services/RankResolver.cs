using System.Collections.Generic;
using System.Linq;

using GinRummy.Domain.Entities;

namespace GinRummy.Domain.Services
{
    // Applies the rule that a score belongs to the rank tier whose score range contains it.
    public class RankResolver
    {
        public Rank ResolveRank(int score, IList<Rank> ranks)
        {
            Rank matchingRank = ranks.FirstOrDefault(rank => (score >= rank.MinimumScore) && (score <= rank.MaximumScore));

            return matchingRank;
        }
    }
}
