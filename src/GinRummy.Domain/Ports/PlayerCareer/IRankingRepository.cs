using System.Collections.Generic;

using GinRummy.Domain.Entities.PlayerCareer;

namespace GinRummy.Domain.Ports.PlayerCareer
{
    public interface IRankingRepository
    {
        IList<PlayerStats> GetStatsRankedByScore();

        IDictionary<int, string> GetUsernamesByPlayerAccountId();

        IList<Rank> GetAllRanks();
    }
}
