using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GinRummy.Domain.Entities;

namespace GinRummy.Domain.Daos
{
    // Contract the game's logic uses to build the leaderboard, without knowing how or where the
    // data is stored.
    public interface IRankingDao
    {
        IList<PlayerStats> GetStatsRankedByScore();

        IList<Rank> GetAllRanks();
    }
}
