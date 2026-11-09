using System.Collections.Generic;

using GinRummy.Domain.Entities.PlayerCareer;

namespace GinRummy.Application.Ports.PlayerCareer
{
    public interface ILeaderboardPort
    {
        IReadOnlyList<LeaderboardPosition> GetLeaderboard();
    }
}
