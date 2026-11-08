using System.Collections.Generic;

using GinRummy.Domain.Entities.PlayerCareer;

namespace GinRummy.Application.Ports
{
    public interface ILeaderboardPort
    {
        IReadOnlyList<LeaderboardPosition> GetLeaderboard();
    }
}
