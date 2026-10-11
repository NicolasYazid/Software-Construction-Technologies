using System.Collections.Generic;

using GinRummy.Domain.Entities;

namespace GinRummy.Domain.Repositories
{
    public interface IRankingRepository
    {
        IList<PlayerStats> GetStatsRankedByScore();

        IList<Rank> GetAllRanks();
    }
}
