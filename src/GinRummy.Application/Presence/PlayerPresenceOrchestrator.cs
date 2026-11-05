using System;
using System.Collections.Generic;
using System.Linq;

using GinRummy.Application.Ports;
using GinRummy.Domain;
using GinRummy.Domain.Entities.Friendship;
using GinRummy.Domain.Entities.PlayerRanking;
using GinRummy.Domain.Entities.Players;
using GinRummy.Domain.Ports.Repositories;
using GinRummy.Domain.Services;

namespace GinRummy.Application.Presence
{
    public class PlayerPresenceOrchestrator : IPlayerPresenceOrchestrator
    {
        private readonly Func<IUnitOfWork> _unitOfWorkFactory;
        private readonly AccountAccessPolicy _accessPolicy;
        private readonly IPlayerPresenceAnnouncer _announcer;
        private readonly RankResolver _rankResolver;

        public PlayerPresenceOrchestrator(
            Func<IUnitOfWork> unitOfWorkFactory,
            AccountAccessPolicy accessPolicy,
            IPlayerPresenceAnnouncer announcer)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _accessPolicy = accessPolicy ?? throw new ArgumentNullException(nameof(accessPolicy));
            _announcer = announcer ?? throw new ArgumentNullException(nameof(announcer));
            _rankResolver = new RankResolver();
        }

        // A guest is followed like a player, so the lobby of a guest has the same panel of players (CU-02 FA-01).
        public int IdentifyByAccessToken(string accessToken)
        {
            int identityId = _accessPolicy.IdentifyByAccessToken(accessToken);
            RankedPlayer identifiedPlayer;
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                identifiedPlayer = FindRankedPlayer(unitOfWork, identityId);
            }

            if (identifiedPlayer == null)
            {
                throw new InvalidAccessTokenException("The access token does not belong to a player or a guest.");
            }

            return identityId;
        }

        public void AnnouncePlayerStatus(
            int playerWithNewStatusId,
            PlayerPresenceStatus newStatus,
            IList<OnlinePlayer> onlinePlayers)
        {
            RankedPlayer playerWithNewStatus;
            IList<int> friendPlayerIds;
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                playerWithNewStatus = FindRankedPlayer(unitOfWork, playerWithNewStatusId);
                friendPlayerIds = GetFriendPlayerIds(unitOfWork, playerWithNewStatusId);
            }

            if (playerWithNewStatus == null)
            {
                return;
            }

            foreach (OnlinePlayer onlinePlayer in onlinePlayers)
            {
                if (onlinePlayer.PlayerId != playerWithNewStatusId)
                {
                    bool isFriend = friendPlayerIds.Contains(onlinePlayer.PlayerId);
                    PlayerPresenceChange change = new PlayerPresenceChange(playerWithNewStatus, newStatus, isFriend);
                    _announcer.SendPlayerPresenceChange(onlinePlayer.PlayerId, change);
                }
            }
        }

        public void ReportCurrentPresenceStatusOfPlayers(int subscriberId, IList<OnlinePlayer> onlinePlayers)
        {
            IList<Player> registeredPlayers;
            IList<PlayerStats> allStats;
            IList<Rank> ranks;
            IList<RankedPlayer> onlineGuests;
            IList<int> friendPlayerIds;
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                registeredPlayers = unitOfWork.Players.GetAllPlayers();
                allStats = unitOfWork.Rankings.GetStatsRankedByScore();
                ranks = unitOfWork.Rankings.GetAllRanks();
                onlineGuests = FindOtherOnlineGuests(unitOfWork, onlinePlayers, subscriberId);
                friendPlayerIds = GetFriendPlayerIds(unitOfWork, subscriberId);
            }

            IList<Player> otherPlayers = GetPlayersExcept(registeredPlayers, subscriberId);
            IList<RankedPlayer> rankedPlayers = RankPlayers(otherPlayers, allStats, ranks).Concat(onlineGuests).ToList();
            IList<int> onlinePlayerIds = GetOnlinePlayerIds(onlinePlayers);
            PlayersByStatus lists = new PlayersByStatus(rankedPlayers, onlinePlayerIds, friendPlayerIds);

            _announcer.SendCurrentPresenceStatusOfPlayers(subscriberId, lists);
        }

        // The identity of a guest is the negative of its session id, so its sign tells where to look for it.
        private RankedPlayer FindRankedPlayer(IUnitOfWork unitOfWork, int identityId)
        {
            RankedPlayer rankedPlayer = null;
            if (identityId < 0)
            {
                rankedPlayer = FindGuest(unitOfWork, identityId);
            }
            else
            {
                Player player = unitOfWork.Players.FindById(identityId);
                if (player != null)
                {
                    rankedPlayer = RankPlayer(player, unitOfWork.PlayerAccounts.FindStats(identityId), unitOfWork.Rankings.GetAllRanks());
                }
            }

            return rankedPlayer;
        }

        // A guest leaves no row when it goes offline, so only the guests that are connected are listed.
        private static IList<RankedPlayer> FindOtherOnlineGuests(
            IUnitOfWork unitOfWork,
            IList<OnlinePlayer> onlinePlayers,
            int subscriberId)
        {
            IList<RankedPlayer> onlineGuests = new List<RankedPlayer>();
            foreach (OnlinePlayer onlinePlayer in onlinePlayers)
            {
                bool isOtherGuest = (onlinePlayer.PlayerId < 0) && (onlinePlayer.PlayerId != subscriberId);
                RankedPlayer guest = isOtherGuest ? FindGuest(unitOfWork, onlinePlayer.PlayerId) : null;
                if (guest != null)
                {
                    onlineGuests.Add(guest);
                }
            }

            return onlineGuests;
        }

        private static RankedPlayer FindGuest(IUnitOfWork unitOfWork, int identityId)
        {
            GuestSession guest = unitOfWork.GuestSessions.FindById(-identityId);
            RankedPlayer rankedGuest = null;
            if (guest != null)
            {
                rankedGuest = new RankedPlayer(guest);
            }

            return rankedGuest;
        }

        private IList<RankedPlayer> RankPlayers(IList<Player> players, IList<PlayerStats> allStats, IList<Rank> ranks)
        {
            Dictionary<int, PlayerStats> statsByPlayerId = allStats.ToDictionary(stats => stats.PlayerId);
            IList<RankedPlayer> rankedPlayers = new List<RankedPlayer>();
            foreach (Player player in players)
            {
                PlayerStats stats;
                statsByPlayerId.TryGetValue(player.PlayerId, out stats);
                rankedPlayers.Add(RankPlayer(player, stats, ranks));
            }

            return rankedPlayers;
        }

        // A player without statistics has not played yet, so it is ranked as a score of zero.
        private RankedPlayer RankPlayer(Player player, PlayerStats stats, IList<Rank> ranks)
        {
            int score = 0;
            if (stats != null)
            {
                score = stats.Score;
            }

            return new RankedPlayer(player, score, _rankResolver.ResolveRank(score, ranks));
        }

        private static IList<int> GetFriendPlayerIds(IUnitOfWork unitOfWork, int playerId)
        {
            IList<int> friendPlayerIds = new List<int>();
            IList<FriendShip> friendShips = unitOfWork.Friends.GetFriendShipsOf(playerId);
            foreach (FriendShip friendShip in friendShips)
            {
                friendPlayerIds.Add(friendShip.GetFriendIdOf(playerId));
            }

            return friendPlayerIds;
        }

        private static IList<Player> GetPlayersExcept(IList<Player> players, int excludedPlayerId)
        {
            IList<Player> otherPlayers = new List<Player>();
            foreach (Player player in players)
            {
                if (player.PlayerId != excludedPlayerId)
                {
                    otherPlayers.Add(player);
                }
            }

            return otherPlayers;
        }

        private static IList<int> GetOnlinePlayerIds(IList<OnlinePlayer> onlinePlayers)
        {
            IList<int> onlinePlayerIds = new List<int>();
            foreach (OnlinePlayer onlinePlayer in onlinePlayers)
            {
                onlinePlayerIds.Add(onlinePlayer.PlayerId);
            }

            return onlinePlayerIds;
        }
    }
}
