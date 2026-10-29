using System.Collections.Generic;
using System.ServiceModel;

using GinRummy.Application.Ports;
using GinRummy.Contracts.DataContracts;
using GinRummy.Contracts.Services;
using GinRummy.Domain.Entities;
using GinRummy.Services.Mappers;

namespace GinRummy.Services.Services
{
    /// <summary>
    /// Implements the rankings network contract. Delegates the leaderboard logic to the
    /// leaderboard port and turns each position into the row the client shows.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class RankingsService : IRankingService
    {
        private readonly ILeaderboardPort _leaderboardPort;

        /// <summary>
        /// Builds the service with the leaderboard port the composition root assembled.
        /// </summary>
        /// <param name="leaderboardPort">Port that builds the ordered leaderboard.</param>
        public RankingsService(ILeaderboardPort leaderboardPort)
        {
            _leaderboardPort = leaderboardPort;
        }

        /// <summary>
        /// Gets the current leaderboard as the rows the client shows.
        /// </summary>
        /// <returns>The ranking rows; empty when there are no players.</returns>
        public List<PlayerRankingDto> GetRankings()
        {
            IReadOnlyList<LeaderboardPosition> leaderboardPositions = _leaderboardPort.GetLeaderboard();
            List<PlayerRankingDto> rankings = PlayerRankingMapper.MapToPlayerRankings(leaderboardPositions);

            return rankings;
        }
    }
}
