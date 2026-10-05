using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GinRummy.Domain.Entities;

namespace GinRummy.Domain.Services
{
    // Resolves which rank a score belongs to, applying the rule that a rank is the tier whose
    // score range contains the score.
    public class RankResolver
    {
        public Rank ResolveRank(int score, IList<Rank> ranks)
        {
            Rank matchingRank = ranks.FirstOrDefault(rank => (score >= rank.MinimumScore) && (score <= rank.MaximumScore));

            return matchingRank;
        }
    }
}
