using System.Collections.Generic;

using GinRummy.Domain.Entities.PlayerRanking;

namespace GinRummy.Application.Ports
{
    public interface ILeaderboardPort
    {
        IReadOnlyList<LeaderboardPosition> GetLeaderboard();
    }
}
