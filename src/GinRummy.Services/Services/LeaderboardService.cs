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
    /// <summary>
    /// Implements the leaderboard contract. Delegates the leaderboard logic to its port and turns each
    /// position into the row the client shows.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class LeaderboardService : ILeaderboardService
    {
        private readonly ILeaderboardPort _leaderboardPort;

        /// <summary>
        /// Builds the service with the leaderboard port the composition root assembled.
        /// </summary>
        /// <param name="leaderboardPort">Port that builds the ordered leaderboard.</param>
        /// <exception cref="ArgumentNullException">Thrown when the port is null.</exception>
        public LeaderboardService(ILeaderboardPort leaderboardPort)
        {
            _leaderboardPort = leaderboardPort ?? throw new ArgumentNullException(nameof(leaderboardPort));
        }

        /// <summary>
        /// Gets the global leaderboard as the rows the client shows.
        /// </summary>
        /// <returns>The rows of the leaderboard; empty when there are no players.</returns>
        public List<PlayerRanking> GetLeaderboard()
        {
            IReadOnlyList<LeaderboardPosition> leaderboardPositions = _leaderboardPort.GetLeaderboard();
            List<PlayerRanking> rankings = PlayerRankingMapper.MapToPlayerRankings(leaderboardPositions);

            return rankings;
        }
    }
}