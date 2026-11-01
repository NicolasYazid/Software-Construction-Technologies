using System;
using System.Collections.Generic;
using System.ServiceModel;

using GinRummy.Application.Ports;
using GinRummy.Contracts.Callbacks;
using GinRummy.Contracts.DataContracts;
using GinRummy.Contracts.Services;
using GinRummy.Domain.Entities;
using GinRummy.Domain.Enums;
using GinRummy.Domain.Repositories;
using GinRummy.Services.Mappers;

namespace GinRummy.Services.Services
{
    /// <summary>
    /// Implements the duplex presence contract. Each session registers its callback channel with
    /// the shared notifier, drives the coordinator by internal id and maps the domain state back to
    /// the presence rows the client shows, the same way LeaderboardService maps the domain to the contract.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class PlayerPresenceService : IPlayerPresenceService
    {
        private readonly IPlayerPresencePort _playerPresencePort;
        private readonly FriendPresenceNotifier _friendPresenceNotifier;
        private readonly IPlayerRepository _playerRepository;

        /// <summary>
        /// Builds the service with the shared singletons and the player repository the composition root assembled.
        /// </summary>
        /// <param name="playerPresencePort">Port that keeps who is connected and resolves friends' presence.</param>
        /// <param name="friendPresenceNotifier">Shared notifier that keeps each player's callback channel.</param>
        /// <param name="playerRepository">Repository used to resolve the subscriber and their friends.</param>
        /// <exception cref="ArgumentNullException">Thrown when a dependency is null.</exception>
        public PlayerPresenceService(
            IPlayerPresencePort playerPresencePort,
            FriendPresenceNotifier friendPresenceNotifier,
            IPlayerRepository playerRepository)
        {
            _playerPresencePort = playerPresencePort ?? throw new ArgumentNullException(nameof(playerPresencePort));
            _friendPresenceNotifier = friendPresenceNotifier ?? throw new ArgumentNullException(nameof(friendPresenceNotifier));
            _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
        }

        /// <summary>
        /// Subscribes the player to the presence of their friends and gets the current state of each one.
        /// </summary>
        /// <param name="subscriberPublicTag">Public tag of the player that subscribes.</param>
        /// <returns>The presence of each friend; empty when the player has no friends.</returns>
        /// <exception cref="FaultException">Thrown when no player has the given public tag.</exception>
        // subscriberPublicTag: in a full design this would come from the authenticated session, not a parameter.
        public List<FriendPresence> SubscribeToFriendsPresence(int subscriberPublicTag)
        {
            Player subscriber = _playerRepository.FindByPublicTag(subscriberPublicTag);
            if (subscriber == null)
            {
                throw new FaultException("The player does not exist.");
            }

            int subscriberId = subscriber.PlayerId;
            IPlayerPresenceCallback callback = OperationContext.Current.GetCallbackChannel<IPlayerPresenceCallback>();
            _friendPresenceNotifier.Register(subscriberId, callback);
            RegisterChannelCleanup(subscriberId);
            IReadOnlyDictionary<int, PresenceState> friendsPresence = _playerPresencePort.SubscribeToFriendsPresence(subscriberId);
            List<FriendPresence> presences = BuildPresences(friendsPresence);

            return presences;
        }

        /// <summary>
        /// Unsubscribes the player, so their friends see them go offline.
        /// </summary>
        /// <param name="subscriberPublicTag">Public tag of the player that unsubscribes.</param>
        public void UnsubscribeFromFriendsPresence(int subscriberPublicTag)
        {
            Player subscriber = _playerRepository.FindByPublicTag(subscriberPublicTag);
            if (subscriber == null)
            {
                return;
            }

            int subscriberId = subscriber.PlayerId;
            _friendPresenceNotifier.Unregister(subscriberId);
            _playerPresencePort.UnsubscribeFromFriendsPresence(subscriberId);
        }

        private List<FriendPresence> BuildPresences(IReadOnlyDictionary<int, PresenceState> friendsPresence)
        {
            List<FriendPresence> presences = new List<FriendPresence>();
            foreach (KeyValuePair<int, PresenceState> friendPresence in friendsPresence)
            {
                Player friend = _playerRepository.FindById(friendPresence.Key);
                FriendPresence presence = FriendPresenceMapper.ToFriendPresence(
                    friend.PublicTag, friend.Username, friendPresence.Value);
                presences.Add(presence);
            }

            return presences;
        }

        private void RegisterChannelCleanup(int subscriberId)
        {
            IContextChannel channel = OperationContext.Current.Channel;
            channel.Closed += (sender, args) => CleanUpSubscription(subscriberId);
            channel.Faulted += (sender, args) => CleanUpSubscription(subscriberId);
        }

        // When the client closes or its channel faults, mark it offline so friends are notified live.
        private void CleanUpSubscription(int subscriberId)
        {
            _friendPresenceNotifier.Unregister(subscriberId);
            _playerPresencePort.UnsubscribeFromFriendsPresence(subscriberId);
        }
    }
}
