using System;
using System.Collections.Generic;

using GinRummy.Application.Ports;
using GinRummy.Domain.Entities;
using GinRummy.Domain.Enums;
using GinRummy.Domain.Repositories;

namespace GinRummy.Application
{
    /// <summary>
    /// Keeps, in memory, which players have an open presence subscription, and answers a new
    /// subscriber with the current state of each of their friends while pushing that
    /// subscriber's own change to the friends who are connected.
    /// </summary>
    public class PresenceCoordinator : IPlayerPresencePort
    {
        private readonly IFriendRepository _friendRepository;
        private readonly IFriendPresenceNotificationPort _friendPresenceNotificationPort;

        // The duplex service receives connects and disconnects on several threads, so a lock guards the connected-players set.
        private readonly HashSet<int> _connectedPlayerIds;
        private readonly object _connectionLock;

        /// <summary>
        /// Builds the coordinator with the friend repository and the notification port the composition root assembled.
        /// </summary>
        /// <param name="friendRepository">Repository used to read a player's friendships.</param>
        /// <param name="friendPresenceNotificationPort">Output port used to push a presence change to a friend.</param>
        /// <exception cref="ArgumentNullException">Thrown when a dependency is null.</exception>
        public PresenceCoordinator(
            IFriendRepository friendRepository,
            IFriendPresenceNotificationPort friendPresenceNotificationPort)
        {
            _friendRepository = friendRepository ?? throw new ArgumentNullException(nameof(friendRepository));
            _friendPresenceNotificationPort = friendPresenceNotificationPort
                ?? throw new ArgumentNullException(nameof(friendPresenceNotificationPort));
            _connectedPlayerIds = new HashSet<int>();
            _connectionLock = new object();
        }

        /// <summary>
        /// Marks the player as connected and returns the current state of each of their friends.
        /// </summary>
        /// <param name="playerId">Player that connects.</param>
        /// <returns>Each friend's internal id paired with their state; empty when the player has no friends.</returns>
        public IReadOnlyDictionary<int, PresenceState> SubscribeToFriendsPresence(int playerId)
        {
            IList<FriendShip> friendShips = _friendRepository.GetFriendShipsOf(playerId);

            MarkConnected(playerId);
            NotifyFriends(friendShips, playerId, PresenceState.Online);
            IReadOnlyDictionary<int, PresenceState> friendsPresence = BuildFriendsPresence(friendShips, playerId);

            return friendsPresence;
        }

        /// <summary>
        /// Marks the player as disconnected and pushes the change to their connected friends.
        /// </summary>
        /// <param name="playerId">Player that disconnects.</param>
        public void UnsubscribeFromFriendsPresence(int playerId)
        {
            IList<FriendShip> friendShips = _friendRepository.GetFriendShipsOf(playerId);

            MarkDisconnected(playerId);
            NotifyFriends(friendShips, playerId, PresenceState.Offline);
        }

        private void MarkConnected(int playerId)
        {
            lock (_connectionLock)
            {
                _connectedPlayerIds.Add(playerId);
            }
        }

        private void MarkDisconnected(int playerId)
        {
            lock (_connectionLock)
            {
                _connectedPlayerIds.Remove(playerId);
            }
        }

        private bool IsConnected(int playerId)
        {
            bool isConnected;
            lock (_connectionLock)
            {
                isConnected = _connectedPlayerIds.Contains(playerId);
            }

            return isConnected;
        }

        private IReadOnlyDictionary<int, PresenceState> BuildFriendsPresence(IList<FriendShip> friendShips, int playerId)
        {
            Dictionary<int, PresenceState> friendsPresence = new Dictionary<int, PresenceState>();
            foreach (FriendShip friendShip in friendShips)
            {
                int friendId = friendShip.GetFriendIdOf(playerId);
                PresenceState friendState = ResolvePresenceState(friendId);
                friendsPresence[friendId] = friendState;
            }

            return friendsPresence;
        }

        private PresenceState ResolvePresenceState(int playerId)
        {
            PresenceState presenceState = PresenceState.Offline;
            if (IsConnected(playerId))
            {
                presenceState = PresenceState.Online;
            }

            return presenceState;
        }

        private void NotifyFriends(IList<FriendShip> friendShips, int changedPlayerId, PresenceState state)
        {
            foreach (FriendShip friendShip in friendShips)
            {
                int friendId = friendShip.GetFriendIdOf(changedPlayerId);

                // Only a friend with an open session has a callback channel, so a disconnected friend is skipped.
                if (IsConnected(friendId))
                {
                    _friendPresenceNotificationPort.NotifyPresenceChanged(friendId, changedPlayerId, state);
                }
            }
        }
    }
}
