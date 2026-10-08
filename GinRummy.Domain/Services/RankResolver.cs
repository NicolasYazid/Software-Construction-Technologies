using System.Collections.Generic;
using System.Linq;

using GinRummy.Domain.Entities;

namespace GinRummy.Domain.Services
{
    public class RankResolver
    {
        public Rank ResolveRank(int score, IList<Rank> ranks)
        {
            Rank matchingRank = ranks.FirstOrDefault(rank => (score >= rank.MinimumScore) && (score <= rank.MaximumScore));

            return matchingRank;
        }
    }
}
