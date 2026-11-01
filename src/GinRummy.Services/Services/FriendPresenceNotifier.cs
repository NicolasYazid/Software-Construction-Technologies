using System;
using System.Collections.Concurrent;
using System.ServiceModel;

using GinRummy.Application.Ports;
using GinRummy.Contracts.Callbacks;
using GinRummy.Contracts.DataContracts;
using GinRummy.Domain.Entities;
using GinRummy.Domain.Enums;
using GinRummy.Domain.Repositories;
using GinRummy.Services.Mappers;

namespace GinRummy.Services.Services
{
    /// <summary>
    /// Shared singleton that implements the output port and also keeps the callback channel of
    /// each connected player, so the coordinator can push a change to the player that must receive
    /// it. It resolves the changed player's public tag and name with the player repository, the
    /// same way LeaderboardService maps the domain to the contract.
    /// </summary>
    public class FriendPresenceNotifier : IFriendPresenceNotificationPort
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly ConcurrentDictionary<int, IPlayerPresenceCallback> _callbacksByPlayerId;

        /// <summary>
        /// Builds the notifier with the player repository the composition root assembled.
        /// </summary>
        /// <param name="playerRepository">Repository used to read a changed player's public tag and name.</param>
        /// <exception cref="ArgumentNullException">Thrown when the repository is null.</exception>
        public FriendPresenceNotifier(IPlayerRepository playerRepository)
        {
            _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
            _callbacksByPlayerId = new ConcurrentDictionary<int, IPlayerPresenceCallback>();
        }

        /// <summary>
        /// Keeps the callback channel of a player that just subscribed.
        /// </summary>
        /// <param name="playerId">Internal id of the player.</param>
        /// <param name="callback">Callback channel of the player's session.</param>
        public void Register(int playerId, IPlayerPresenceCallback callback)
        {
            _callbacksByPlayerId[playerId] = callback;
        }

        /// <summary>
        /// Drops the callback channel of a player that unsubscribed or whose channel died.
        /// </summary>
        /// <param name="playerId">Internal id of the player.</param>
        public void Unregister(int playerId)
        {
            IPlayerPresenceCallback removed;
            _callbacksByPlayerId.TryRemove(playerId, out removed);
        }

        /// <summary>
        /// Pushes to a recipient that one of their friends changed presence, when the recipient
        /// still has an open callback channel.
        /// </summary>
        /// <param name="recipientPlayerId">Internal id of the player who receives the notice.</param>
        /// <param name="changedPlayerId">Internal id of the player whose presence changed.</param>
        /// <param name="state">New presence state of the changed player.</param>
        public void NotifyPresenceChanged(int recipientPlayerId, int changedPlayerId, PresenceState state)
        {
            IPlayerPresenceCallback callback;
            if (_callbacksByPlayerId.TryGetValue(recipientPlayerId, out callback))
            {
                Player changedPlayer = _playerRepository.FindById(changedPlayerId);
                FriendPresence presence = FriendPresenceMapper.ToFriendPresence(
                    changedPlayer.PublicTag, changedPlayer.Username, state);
                PushSafely(recipientPlayerId, callback, presence);
            }
        }

        private void PushSafely(int recipientPlayerId, IPlayerPresenceCallback callback, FriendPresence presence)
        {
            try
            {
                callback.OnFriendPresenceChanged(presence);
            }
            catch (CommunicationException)
            {
                // The recipient's channel died; drop it so stale callbacks are not kept.
                Unregister(recipientPlayerId);
            }
            catch (TimeoutException)
            {
                // The recipient stopped responding; drop the channel.
                Unregister(recipientPlayerId);
            }
            catch (ObjectDisposedException)
            {
                // The recipient's channel was already disposed; drop it.
                Unregister(recipientPlayerId);
            }
        }
    }
}
