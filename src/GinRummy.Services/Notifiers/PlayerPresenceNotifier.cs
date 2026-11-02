using System;
using System.Collections.Generic;

using GinRummy.Application.Ports;
using GinRummy.Application.Presence;
using GinRummy.Contracts.Callbacks;
using GinRummy.Domain.Entities.Players;
using GinRummy.Services.Mappers;
using Microsoft.Extensions.Logging;

namespace GinRummy.Services.Notifiers
{
    /// <summary>
    /// Fulfils the way out of the monitoring of the players: it keeps the callback channel of every subscribed player and
    /// delivers to each one the notices the application asks for.
    /// </summary>
    public class PlayerPresenceNotifier : IPlayerPresenceNotifier
    {
        private readonly Dictionary<int, SubscribedClient> _subscribedClients;
        // Clients subscribe and leave on several threads at once, so every access takes this lock.
        private readonly object _clientsLock;
        private readonly ILogger<SubscribedClient> _clientLogger;

        /// <summary>
        /// Builds the notifier with the logger every subscribed client will use.
        /// </summary>
        /// <param name="clientLogger">Logger given to each subscribed client.</param>
        /// <exception cref="ArgumentNullException">Thrown when the logger is null.</exception>
        public PlayerPresenceNotifier(ILogger<SubscribedClient> clientLogger)
        {
            _clientLogger = clientLogger ?? throw new ArgumentNullException(nameof(clientLogger));
            _subscribedClients = new Dictionary<int, SubscribedClient>();
            _clientsLock = new object();
        }

        /// <summary>
        /// Keeps the callback channel of a player who subscribed. A channel registered earlier for the same player is stopped
        /// and replaced.
        /// </summary>
        /// <param name="playerId">Identifier of the player who subscribed.</param>
        /// <param name="callback">Callback channel of that player.</param>
        public void RegisterClient(int playerId, IPlayerPresenceCallback callback)
        {
            SubscribedClient newClient = new SubscribedClient(callback, _clientLogger);
            SubscribedClient replacedClient = null;
            lock (_clientsLock)
            {
                if (_subscribedClients.ContainsKey(playerId))
                {
                    replacedClient = _subscribedClients[playerId];
                }

                _subscribedClients[playerId] = newClient;
            }

            if (replacedClient != null)
            {
                replacedClient.Stop();
            }
        }

        /// <summary>
        /// Forgets the callback channel of a player who left. It does nothing when the player had none.
        /// </summary>
        /// <param name="playerId">Identifier of the player who left.</param>
        public void UnregisterClient(int playerId)
        {
            SubscribedClient removedClient = null;
            lock (_clientsLock)
            {
                if (_subscribedClients.ContainsKey(playerId))
                {
                    removedClient = _subscribedClients[playerId];
                    _subscribedClients.Remove(playerId);
                }
            }

            if (removedClient != null)
            {
                removedClient.Stop();
            }
        }

        /// <summary>
        /// Sends a connected player the current status of the players. It does nothing when the player has no channel.
        /// </summary>
        /// <param name="playerId">Identifier of the connected player who receives the lists.</param>
        /// <param name="lists">The players and the friends of that player, split into connected and not connected.</param>
        public void SendCurrentStatusOfPlayers(int playerId, PlayerPresenceLists lists)
        {
            SubscribedClient client = FindClient(playerId);
            if (client == null)
            {
                return;
            }

            client.SendCurrentStatus(PlayerPresenceMapper.MapToPlayerPresenceReport(lists));
        }

        /// <summary>
        /// Tells a connected player that the status of another player changed. It does nothing when the player has no channel.
        /// </summary>
        /// <param name="recipient">Connected player who receives the notice.</param>
        /// <param name="changedPlayer">Player whose status changed.</param>
        /// <param name="status">Status the changed player has now.</param>
        public void SendPlayerPresenceChange(ConnectedPlayer recipient, Player changedPlayer, PlayerPresenceStatus status)
        {
            SubscribedClient client = FindClient(recipient.Player.PlayerId);
            if (client == null)
            {
                return;
            }

            bool isFriend = recipient.IsFriendOf(changedPlayer.PlayerId);
            client.SendPlayerPresence(PlayerPresenceMapper.MapToPlayerPresence(changedPlayer, status, isFriend));
        }

        private SubscribedClient FindClient(int playerId)
        {
            SubscribedClient foundClient = null;
            lock (_clientsLock)
            {
                if (_subscribedClients.ContainsKey(playerId))
                {
                    foundClient = _subscribedClients[playerId];
                }
            }

            return foundClient;
        }
    }
}
