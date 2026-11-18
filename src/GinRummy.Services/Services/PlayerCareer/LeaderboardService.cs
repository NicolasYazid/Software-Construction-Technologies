using System;
using System.Collections.Generic;
using System.ServiceModel;

using GinRummy.Application.Ports.PlayerCareer;
using GinRummy.Contracts.DataContracts.PlayerCareer;
using GinRummy.Contracts.Services.PlayerCareer;
using GinRummy.Domain.Entities.Authentication;
using GinRummy.Domain.Entities.PlayerCareer;
using GinRummy.Services.Mappers.PlayerCareer;
using ContractLeaderboardType = GinRummy.Contracts.DataContracts.PlayerCareer.LeaderboardType;
using DomainLeaderboardType = GinRummy.Domain.Entities.PlayerCareer.LeaderboardType;

namespace GinRummy.Services.Services.PlayerCareer
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class LeaderboardService : ILeaderboardService
    {
        private readonly AccountAccessPolicy _accessPolicy;
        private readonly IViewLeaderboardUseCase _viewLeaderboardUseCase;

        public LeaderboardService(AccountAccessPolicy accessPolicy, IViewLeaderboardUseCase viewLeaderboardUseCase)
        {
            _accessPolicy = accessPolicy ?? throw new ArgumentNullException(nameof(accessPolicy));
            _viewLeaderboardUseCase = viewLeaderboardUseCase ?? throw new ArgumentNullException(nameof(viewLeaderboardUseCase));
        }

        public List<PlayerRanking> GetLeaderboard(string accessToken, ContractLeaderboardType leaderboardType)
        {
            int playerId = _accessPolicy.IdentifyPlayerByAccessToken(accessToken);
            DomainLeaderboardType domainLeaderboardType = PlayerRankingMapper.MapToDomainLeaderboardType(leaderboardType);
            IReadOnlyList<LeaderboardPlayerPosition> leaderboardPositions =
                _viewLeaderboardUseCase.GetLeaderboard(playerId, domainLeaderboardType);
            List<PlayerRanking> rankings = PlayerRankingMapper.MapToPlayerRankings(leaderboardPositions);

            return rankings;
        }
    }
}
