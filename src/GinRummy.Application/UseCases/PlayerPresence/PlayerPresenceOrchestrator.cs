using System;
using System.Collections.Generic;
using System.Linq;

using GinRummy.Application.Ports.PlayerPresence;
using GinRummy.Domain;
using GinRummy.Domain.Entities.Authentication;
using GinRummy.Domain.Entities.Friendship;
using GinRummy.Domain.Entities.PlayerCareer;
using GinRummy.Domain.Entities.PlayerCustomization;
using GinRummy.Domain.Entities.PlayerPresence;
using GinRummy.Domain.Shared;

namespace GinRummy.Application.UseCases.PlayerPresence
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

        public AccessTokenOwner IdentifyByAccessToken(string accessToken)
        {
            AccessTokenOwner owner = _accessPolicy.IdentifyByAccessToken(accessToken);
            RankedPlayer identifiedPlayer;
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                identifiedPlayer = FindRankedPlayer(unitOfWork, owner);
            }

            if (identifiedPlayer == null)
            {
                throw new InvalidAccessTokenException("The access token does not belong to a player or a guest.");
            }

            return owner;
        }

        public void AnnouncePlayerStatus(
            AccessTokenOwner playerWithNewStatus,
            PlayerPresenceStatus newStatus,
            IList<OnlinePlayer> onlinePlayers)
        {
            RankedPlayer rankedPlayerWithNewStatus;
            IList<int> friendPlayerAccountIds;
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                rankedPlayerWithNewStatus = FindRankedPlayer(unitOfWork, playerWithNewStatus);
                friendPlayerAccountIds = GetFriendPlayerAccountIds(unitOfWork, playerWithNewStatus);
            }

            if (rankedPlayerWithNewStatus == null)
            {
                return;
            }

            foreach (OnlinePlayer onlinePlayer in onlinePlayers)
            {
                if (!onlinePlayer.AccessTokenOwner.Equals(playerWithNewStatus))
                {
                    bool isFriend = onlinePlayer.AccessTokenOwner.PlayerAccountId.HasValue
                        && friendPlayerAccountIds.Contains(onlinePlayer.AccessTokenOwner.PlayerAccountId.Value);
                    PlayerPresenceChange change = new PlayerPresenceChange(rankedPlayerWithNewStatus, newStatus, isFriend);
                    _announcer.SendPlayerPresenceChange(onlinePlayer.AccessTokenOwner, change);
                }
            }
        }

        public void ReportCurrentPresenceStatusOfPlayers(AccessTokenOwner subscriber, IList<OnlinePlayer> onlinePlayers)
        {
            IList<PlayerProfile> registeredPlayers;
            IList<PlayerStats> allStats;
            IList<Rank> ranks;
            IList<RankedPlayer> onlineGuests;
            IList<int> friendPlayerAccountIds;
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                registeredPlayers = unitOfWork.PlayerAccounts.GetAllPlayers();
                allStats = unitOfWork.Rankings.GetStatsRankedByScore();
                ranks = unitOfWork.Rankings.GetAllRanks();
                onlineGuests = FindOtherOnlineGuests(unitOfWork, onlinePlayers, subscriber);
                friendPlayerAccountIds = GetFriendPlayerAccountIds(unitOfWork, subscriber);
            }

            IList<PlayerProfile> otherPlayers = GetPlayersExcept(registeredPlayers, subscriber);
            IList<RankedPlayer> rankedPlayers = RankPlayers(otherPlayers, allStats, ranks).Concat(onlineGuests).ToList();
            IList<AccessTokenOwner> onlineOwners = GetOnlineOwners(onlinePlayers);
            PlayersByStatus lists = new PlayersByStatus(rankedPlayers, onlineOwners, friendPlayerAccountIds);

            _announcer.SendCurrentPresenceStatusOfPlayers(subscriber, lists);
        }

        private RankedPlayer FindRankedPlayer(IUnitOfWork unitOfWork, AccessTokenOwner owner)
        {
            RankedPlayer rankedPlayer = null;
            if (owner.IsGuest)
            {
                rankedPlayer = FindGuest(unitOfWork, owner.GuestSessionId.Value);
            }
            else
            {
                int playerAccountId = owner.PlayerAccountId.Value;
                PlayerProfile player = unitOfWork.PlayerAccounts.FindProfileByPlayerAccountId(playerAccountId);
                if (player != null)
                {
                    rankedPlayer = RankPlayer(player, unitOfWork.PlayerAccounts.FindStats(playerAccountId), unitOfWork.Rankings.GetAllRanks());
                }
            }

            return rankedPlayer;
        }

        private static IList<RankedPlayer> FindOtherOnlineGuests(
            IUnitOfWork unitOfWork,
            IList<OnlinePlayer> onlinePlayers,
            AccessTokenOwner subscriber)
        {
            IList<RankedPlayer> onlineGuests = new List<RankedPlayer>();
            foreach (OnlinePlayer onlinePlayer in onlinePlayers)
            {
                AccessTokenOwner owner = onlinePlayer.AccessTokenOwner;
                bool isOtherGuest = owner.IsGuest && !owner.Equals(subscriber);
                RankedPlayer guest = isOtherGuest ? FindGuest(unitOfWork, owner.GuestSessionId.Value) : null;
                if (guest != null)
                {
                    onlineGuests.Add(guest);
                }
            }

            return onlineGuests;
        }

        private static RankedPlayer FindGuest(IUnitOfWork unitOfWork, int guestSessionId)
        {
            GuestSession guest = unitOfWork.GuestSessions.FindById(guestSessionId);
            RankedPlayer rankedGuest = null;
            if (guest != null)
            {
                rankedGuest = new RankedPlayer(guest);
            }

            return rankedGuest;
        }

        private IList<RankedPlayer> RankPlayers(IList<PlayerProfile> players, IList<PlayerStats> allStats, IList<Rank> ranks)
        {
            Dictionary<int, PlayerStats> statsByPlayerAccountId = allStats.ToDictionary(stats => stats.PlayerAccountId);
            IList<RankedPlayer> rankedPlayers = new List<RankedPlayer>();
            foreach (PlayerProfile player in players)
            {
                PlayerStats stats;
                statsByPlayerAccountId.TryGetValue(player.PlayerAccountId, out stats);
                rankedPlayers.Add(RankPlayer(player, stats, ranks));
            }

            return rankedPlayers;
        }

        private RankedPlayer RankPlayer(PlayerProfile player, PlayerStats stats, IList<Rank> ranks)
        {
            int score = 0;
            if (stats != null)
            {
                score = stats.Score;
            }

            return new RankedPlayer(player, score, _rankResolver.ResolveRank(score, ranks));
        }

        private static IList<int> GetFriendPlayerAccountIds(IUnitOfWork unitOfWork, AccessTokenOwner owner)
        {
            IList<int> friendPlayerAccountIds = new List<int>();
            if (!owner.IsGuest)
            {
                int playerAccountId = owner.PlayerAccountId.Value;
                IList<FriendShip> friendShips = unitOfWork.Friends.GetFriendShipsOf(playerAccountId);
                foreach (FriendShip friendShip in friendShips)
                {
                    friendPlayerAccountIds.Add(friendShip.GetFriendIdOf(playerAccountId));
                }
            }

            return friendPlayerAccountIds;
        }

        private static IList<PlayerProfile> GetPlayersExcept(IList<PlayerProfile> players, AccessTokenOwner excludedOwner)
        {
            IList<PlayerProfile> otherPlayers = new List<PlayerProfile>();
            foreach (PlayerProfile player in players)
            {
                if (excludedOwner.PlayerAccountId != player.PlayerAccountId)
                {
                    otherPlayers.Add(player);
                }
            }

            return otherPlayers;
        }

        private static IList<AccessTokenOwner> GetOnlineOwners(IList<OnlinePlayer> onlinePlayers)
        {
            IList<AccessTokenOwner> onlineOwners = new List<AccessTokenOwner>();
            foreach (OnlinePlayer onlinePlayer in onlinePlayers)
            {
                onlineOwners.Add(onlinePlayer.AccessTokenOwner);
            }

            return onlineOwners;
        }
    }
}
