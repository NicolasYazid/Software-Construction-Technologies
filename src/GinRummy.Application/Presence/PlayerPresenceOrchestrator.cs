using System;
using System.Collections.Generic;

using GinRummy.Application.Ports;
using GinRummy.Domain;
using GinRummy.Domain.Entities.Friendship;
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

        public PlayerPresenceOrchestrator(
            Func<IUnitOfWork> unitOfWorkFactory,
            AccountAccessPolicy accessPolicy,
            IPlayerPresenceAnnouncer announcer)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _accessPolicy = accessPolicy ?? throw new ArgumentNullException(nameof(accessPolicy));
            _announcer = announcer ?? throw new ArgumentNullException(nameof(announcer));
        }

        public int IdentifyPlayerByAccessToken(string accessToken)
        {
            int playerId = _accessPolicy.IdentifyPlayerByAccessToken(accessToken);
            Player player;
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                player = unitOfWork.Players.FindById(playerId);
            }

            if (player == null)
            {
                throw new InvalidAccessTokenException("The access token does not belong to a registered player.");
            }

            return playerId;
        }

        public void AnnouncePlayerStatus(
            int playerWithNewStatusId,
            PlayerPresenceStatus newStatus,
            IList<OnlinePlayer> onlinePlayers)
        {
            Player playerWithNewStatus;
            IList<int> friendPlayerIds;
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                playerWithNewStatus = unitOfWork.Players.FindById(playerWithNewStatusId);
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
            IList<int> friendPlayerIds;
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                registeredPlayers = unitOfWork.Players.GetAllPlayers();
                friendPlayerIds = GetFriendPlayerIds(unitOfWork, subscriberId);
            }

            IList<Player> otherPlayers = GetPlayersExcept(registeredPlayers, subscriberId);
            IList<int> onlinePlayerIds = GetOnlinePlayerIds(onlinePlayers);
            PlayersByStatus lists = new PlayersByStatus(otherPlayers, onlinePlayerIds, friendPlayerIds);

            _announcer.SendCurrentPresenceStatusOfPlayers(subscriberId, lists);
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
