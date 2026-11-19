using System;
using System.Collections.Generic;

using GinRummy.Application.Ports.PlayerCareer;
using GinRummy.Domain.Entities.Friendship;
using GinRummy.Domain.Entities.PlayerCareer;
using GinRummy.Domain.Ports.PlayerCareer;
using GinRummy.Domain.Shared;

namespace GinRummy.Application.UseCases.PlayerCareer
{
    public class ViewLeaderboardUseCase : IViewLeaderboardUseCase
    {
        private const int FirstPlace = 1;

        private readonly IRankingRepository _rankingRepository;
        private readonly RankResolver _rankResolver;
        private readonly Func<IUnitOfWork> _unitOfWorkFactory;

        public ViewLeaderboardUseCase(IRankingRepository rankingRepository, RankResolver rankResolver, Func<IUnitOfWork> unitOfWorkFactory)
        {
            _rankingRepository = rankingRepository ?? throw new ArgumentNullException(nameof(rankingRepository));
            _rankResolver = rankResolver ?? throw new ArgumentNullException(nameof(rankResolver));
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
        }

        public IReadOnlyList<LeaderboardPlayerPosition> GetLeaderboard(int playerId, LeaderboardType leaderboardType)
        {
            IList<PlayerStats> stats;
            IDictionary<int, string> usernames;
            if (leaderboardType == LeaderboardType.Friends)
            {
                IList<int> playerAccountIds = FindPlayerAndFriendIds(playerId);
                stats = _rankingRepository.GetStatsOfPlayers(playerAccountIds);
                usernames = _rankingRepository.GetUsernamesOfPlayers(playerAccountIds);
            }
            else
            {
                stats = _rankingRepository.GetStatsRankedByScore();
                usernames = _rankingRepository.GetUsernamesByPlayerAccountId();
            }

            IList<Rank> ranks = _rankingRepository.GetAllRanks();
            IReadOnlyList<LeaderboardPlayerPosition> leaderboard = BuildLeaderboard(stats, usernames, ranks);

            return leaderboard;
        }

        private IList<int> FindPlayerAndFriendIds(int playerId)
        {
            List<int> playerAccountIds = new List<int>();
            playerAccountIds.Add(playerId);
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                foreach (FriendShip friendShip in unitOfWork.Friends.GetFriendShipsOf(playerId))
                {
                    playerAccountIds.Add(friendShip.GetFriendIdOf(playerId));
                }
            }

            return playerAccountIds;
        }

        private IReadOnlyList<LeaderboardPlayerPosition> BuildLeaderboard(
            IList<PlayerStats> stats,
            IDictionary<int, string> usernames,
            IList<Rank> ranks)
        {
            List<LeaderboardPlayerPosition> leaderboard = new List<LeaderboardPlayerPosition>();
            IList<PlayerStats> orderedStats = LeaderboardRules.OrderByLeaderboardRules(stats, usernames);
            int place = FirstPlace;
            foreach (PlayerStats playerStats in orderedStats)
            {
                Rank rank = _rankResolver.ResolveRank(playerStats.Score, ranks);
                string username = usernames[playerStats.PlayerAccountId];
                leaderboard.Add(new LeaderboardPlayerPosition(place, playerStats, new RankedUsername(username, rank)));
                place++;
            }

            return leaderboard;
        }
    }
}
