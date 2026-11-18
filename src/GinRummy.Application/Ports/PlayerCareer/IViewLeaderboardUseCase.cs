using System.Collections.Generic;

using GinRummy.Domain.Entities.PlayerCareer;

namespace GinRummy.Application.Ports.PlayerCareer
{
    public interface IViewLeaderboardUseCase
    {
        IReadOnlyList<LeaderboardPlayerPosition> GetLeaderboard(int playerId, LeaderboardType leaderboardType);
    }
}
