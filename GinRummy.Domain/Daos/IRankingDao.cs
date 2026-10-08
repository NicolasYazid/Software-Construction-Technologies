using System.Collections.Generic;

using GinRummy.Domain.Entities;

namespace GinRummy.Domain.Daos
{
    // Contract the game's logic uses to build the leaderboard.
    // The logic does not need to know how or where the data is stored.
    public interface IRankingDao
    {
        IList<PlayerStats> GetStatsRankedByScore();

        IList<Rank> GetAllRanks();
    }
}
