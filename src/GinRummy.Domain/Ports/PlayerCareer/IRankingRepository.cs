using System.Collections.Generic;

using GinRummy.Domain.Entities.PlayerCareer;

namespace GinRummy.Domain.Ports.PlayerCareer
{
    public interface IRankingRepository
    {
        IList<PlayerStats> GetStatsRankedByScore();

        IDictionary<int, string> GetUsernamesByPlayerAccountId();

        IList<Rank> GetAllRanks();

        IList<PlayerStats> GetStatsOfPlayers(IList<int> playerAccountIds);

        IDictionary<int, string> GetUsernamesOfPlayers(IList<int> playerAccountIds);
    }
}
