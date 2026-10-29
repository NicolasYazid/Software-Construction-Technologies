using System.Collections.Generic;
using System.ServiceModel;

using GinRummy.Application.Ports;
using GinRummy.Contracts.DataContracts;
using GinRummy.Contracts.Services;
using GinRummy.Domain.Entities;

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
            List<PlayerRankingDto> rankings = MapToDtos(leaderboardPositions);

            return rankings;
        }

        private List<PlayerRankingDto> MapToDtos(IReadOnlyList<LeaderboardPosition> leaderboardPositions)
        {
            List<PlayerRankingDto> rankings = new List<PlayerRankingDto>();
            foreach (LeaderboardPosition position in leaderboardPositions)
            {
                rankings.Add(MapToDto(position));
            }

            return rankings;
        }

        private PlayerRankingDto MapToDto(LeaderboardPosition position)
        {
            PlayerRankingDto ranking = new PlayerRankingDto();
            ranking.Position = position.Place;
            ranking.Username = position.Stats.Player.Username;
            ranking.RankName = ResolveRankName(position.Rank);
            ranking.Wins = position.Stats.Wins;
            ranking.Losses = position.Stats.Losses;
            ranking.WinRate = position.Stats.WinRate;

            return ranking;
        }

        private string ResolveRankName(Rank rank)
        {
            string rankName = string.Empty;
            if (rank != null)
            {
                rankName = rank.Name;
            }

            return rankName;
        }
    }
}
