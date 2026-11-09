using System;
using System.Collections.Generic;
using System.ServiceModel;

using GinRummy.Application.Ports.PlayerCareer;
using GinRummy.Contracts.DataContracts.PlayerCareer;
using GinRummy.Contracts.Services.PlayerCareer;
using GinRummy.Domain.Entities.PlayerCareer;
using GinRummy.Services.Mappers;

namespace GinRummy.Services.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class LeaderboardService : ILeaderboardService
    {
        private readonly ILeaderboardPort _leaderboardPort;

        public LeaderboardService(ILeaderboardPort leaderboardPort)
        {
            _leaderboardPort = leaderboardPort ?? throw new ArgumentNullException(nameof(leaderboardPort));
        }

        public List<PlayerRanking> GetLeaderboard()
        {
            IReadOnlyList<LeaderboardPosition> leaderboardPositions = _leaderboardPort.GetLeaderboard();
            List<PlayerRanking> rankings = PlayerRankingMapper.MapToPlayerRankings(leaderboardPositions);

            return rankings;
        }
    }
}
