using System;
using System.Collections.Generic;
using System.ServiceModel;

using GinRummy.Application.Ports;
using GinRummy.Contracts.DataContracts;
using GinRummy.Contracts.Services;
using GinRummy.Domain.Entities.PlayerRanking;
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
