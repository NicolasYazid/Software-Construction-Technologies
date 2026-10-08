using System.Collections.Generic;

using GinRummy.Domain.Entities;

namespace GinRummy.Domain.Daos
{
    public interface IRankingDao
    {
        IList<PlayerStats> GetStatsRankedByScore();

        IList<Rank> GetAllRanks();
    }
}
