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
    public class PlayerPresenceCoordinator : IPlayerPresencePort
    {
        private readonly Func<IUnitOfWork> _unitOfWorkFactory;
        private readonly AccountAccessPolicy _accessPolicy;
        private readonly IPlayerPresenceNotifier _notifier;
        private readonly ConnectedPlayerRegistry _registry;

        public PlayerPresenceCoordinator(
            Func<IUnitOfWork> unitOfWorkFactory,
            AccountAccessPolicy accessPolicy,
            IPlayerPresenceNotifier notifier)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _accessPolicy = accessPolicy ?? throw new ArgumentNullException(nameof(accessPolicy));
            _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
            _registry = new ConnectedPlayerRegistry();
        }

        public int SubscribeToPlayerPresence(string accessToken)
        {
            int playerId = _accessPolicy.IdentifyPlayer(accessToken);
            ConnectedPlayer subscriber = LoadConnectedPlayer(playerId);

            _registry.AddConnectedPlayer(subscriber);
            NotifyOtherPlayers(subscriber.Player, PlayerPresenceStatus.Online);

            return playerId;
        }

        public void ReportCurrentStatusOfPlayers(int playerId)
        {
            ConnectedPlayer subscriber = _registry.FindConnectedPlayer(playerId);
            if (subscriber == null)
            {
                return;
            }

            IList<Player> registeredPlayers = LoadRegisteredPlayers();
            IList<int> connectedPlayerIds = GetConnectedPlayerIds();
            PlayerPresenceLists lists = new PlayerPresenceLists(registeredPlayers, connectedPlayerIds, subscriber);

            _notifier.SendCurrentStatusOfPlayers(playerId, lists);
        }

        public void UnsubscribeFromPlayerPresence(int playerId)
        {
            ConnectedPlayer removedPlayer = _registry.RemoveConnectedPlayer(playerId);
            if (removedPlayer == null)
            {
                return;
            }

            NotifyOtherPlayers(removedPlayer.Player, PlayerPresenceStatus.Offline);
        }

        private ConnectedPlayer LoadConnectedPlayer(int playerId)
        {
            ConnectedPlayer connectedPlayer;
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                Player player = unitOfWork.Players.FindById(playerId);
                if (player == null)
                {
                    throw new InvalidAccessTokenException("The access token does not belong to a registered player.");
                }

                IList<int> friendPlayerIds = GetFriendPlayerIds(unitOfWork, playerId);
                connectedPlayer = new ConnectedPlayer(player, friendPlayerIds);
            }

            return connectedPlayer;
        }

        private IList<int> GetFriendPlayerIds(IUnitOfWork unitOfWork, int playerId)
        {
            IList<int> friendPlayerIds = new List<int>();
            IList<FriendShip> friendShips = unitOfWork.Friends.GetFriendShipsOf(playerId);
            foreach (FriendShip friendShip in friendShips)
            {
                friendPlayerIds.Add(friendShip.GetFriendIdOf(playerId));
            }

            return friendPlayerIds;
        }

        private IList<Player> LoadRegisteredPlayers()
        {
            IList<Player> registeredPlayers;
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                registeredPlayers = unitOfWork.Players.GetAllPlayers();
            }

            return registeredPlayers;
        }

        private IList<int> GetConnectedPlayerIds()
        {
            IList<int> connectedPlayerIds = new List<int>();
            IList<ConnectedPlayer> connectedPlayers = _registry.GetConnectedPlayers();
            foreach (ConnectedPlayer connectedPlayer in connectedPlayers)
            {
                connectedPlayerIds.Add(connectedPlayer.Player.PlayerId);
            }

            return connectedPlayerIds;
        }

        // The player who changed is skipped because that player already knows their own status.
        private void NotifyOtherPlayers(Player changedPlayer, PlayerPresenceStatus status)
        {
            IList<ConnectedPlayer> connectedPlayers = _registry.GetConnectedPlayers();
            foreach (ConnectedPlayer recipient in connectedPlayers)
            {
                if (recipient.Player.PlayerId != changedPlayer.PlayerId)
                {
                    _notifier.SendPlayerPresenceChange(recipient, changedPlayer, status);
                }
            }
        }
    }
}
