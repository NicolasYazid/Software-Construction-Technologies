using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

using GinRummy.Application.UseCases;
using GinRummy.Contracts;
using GinRummy.Domain.Entities;

namespace GinRummy.Server.Services
{
    /// <summary>
    /// Implements the rankings network contract. It delegates the leaderboard logic to the
    /// View Leaderboard use case and turns each position into the row the client shows.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class RankingsService : IRankingService
    {
        private readonly ViewLeaderboardUseCase _viewLeaderboardUseCase;

        /// <summary>
        /// Builds the service with the View Leaderboard use case the composition root assembled.
        /// </summary>
        /// <param name="viewLeaderboardUseCase">Use case that builds the ordered leaderboard.</param>
        public RankingsService(ViewLeaderboardUseCase viewLeaderboardUseCase)
        {
            _viewLeaderboardUseCase = viewLeaderboardUseCase;
        }

        /// <summary>
        /// Gets the current leaderboard as the rows the client shows.
        /// </summary>
        /// <returns>The ranking rows; empty when there are no players.</returns>
        public List<PlayerRankingDto> GetRankings()
        {
            IReadOnlyList<LeaderboardPosition> leaderboard = _viewLeaderboardUseCase.GetLeaderboard();
            List<PlayerRankingDto> rankings = MapToDtos(leaderboard);

            return rankings;
        }

        private List<PlayerRankingDto> MapToDtos(IReadOnlyList<LeaderboardPosition> leaderboard)
        {
            List<PlayerRankingDto> rankings = new List<PlayerRankingDto>();
            foreach (LeaderboardPosition position in leaderboard)
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
