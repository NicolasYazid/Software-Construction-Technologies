using System;
using System.ServiceModel;

using GinRummy.Application.Ports;
using GinRummy.Contracts.Callbacks;
using GinRummy.Contracts.Faults;
using GinRummy.Contracts.Services;
using GinRummy.Domain;
using GinRummy.Services.Notifiers;
using Microsoft.Extensions.Logging;

namespace GinRummy.Services.Services
{
    /// <summary>
    /// Implements the contract that follows which players are connected. It keeps one instance per client for as long as
    /// the client stays connected, hands the callback channel of the client to the notifier, and tells the application
    /// when the client leaves, whether it asked to or its channel closed or failed.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class PlayerPresenceService : IPlayerPresenceService
    {
        private const int NoPlayerId = 0;

        private readonly IPlayerPresencePort _presencePort;
        private readonly PlayerPresenceNotifier _notifier;
        private readonly ILogger<PlayerPresenceService> _logger;
        // The channel events arrive on other threads than the operations, so the player of this client is read and cleared under a lock.
        private readonly object _playerLock;
        private int _playerId;

        /// <summary>
        /// Builds the service with the port, the notifier and the logger the composition root assembled.
        /// </summary>
        /// <param name="presencePort">Port that keeps track of the connected players.</param>
        /// <param name="notifier">Keeps the callback channel of every subscribed player.</param>
        /// <param name="logger">Logger of the server.</param>
        /// <exception cref="ArgumentNullException">Thrown when a dependency is null.</exception>
        public PlayerPresenceService(
            IPlayerPresencePort presencePort,
            PlayerPresenceNotifier notifier,
            ILogger<PlayerPresenceService> logger)
        {
            _presencePort = presencePort ?? throw new ArgumentNullException(nameof(presencePort));
            _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _playerLock = new object();
            _playerId = NoPlayerId;
        }

        /// <summary>
        /// Subscribes the player the access token belongs to. The server then reports the current status of the
        /// players and every later change through the callback.
        /// </summary>
        /// <param name="accessToken">Signed access token the player received when signing in.</param>
        /// <exception cref="FaultException{ServiceFault}">
        /// Thrown when the access token is malformed, altered or expired.
        /// </exception>
        public void SubscribeToPlayerPresence(string accessToken)
        {
            OperationContext context = OperationContext.Current;
            IPlayerPresenceCallback callback = context.GetCallbackChannel<IPlayerPresenceCallback>();
            try
            {
                int playerId = _presencePort.SubscribeToPlayerPresence(accessToken);
                _notifier.RegisterClient(playerId, callback);
                SavePlayerId(playerId);
                context.Channel.Closed += OnChannelClosed;
                context.Channel.Faulted += OnChannelFaulted;
                _presencePort.ReportCurrentStatusOfPlayers(playerId);
            }
            catch (InvalidAccessTokenException ex)
            {
                _logger.LogWarning(ex, "A client tried to follow the presence of the players with an access token that is not valid.");
                throw CreateServiceFault(ServiceFaultCode.SessionExpired);
            }
        }

        /// <summary>
        /// Stops the notices of the presence of the players. The other players see this player as disconnected.
        /// </summary>
        public void UnsubscribeFromPlayerPresence()
        {
            Disconnect();
        }

        private static FaultException<ServiceFault> CreateServiceFault(ServiceFaultCode code)
        {
            return new FaultException<ServiceFault>(new ServiceFault { Code = code });
        }

        private void SavePlayerId(int playerId)
        {
            lock (_playerLock)
            {
                _playerId = playerId;
            }
        }

        // It is safe to run more than once: the first call takes the player and the rest find nobody to disconnect.
        private void Disconnect()
        {
            int playerId;
            lock (_playerLock)
            {
                playerId = _playerId;
                _playerId = NoPlayerId;
            }

            if (playerId == NoPlayerId)
            {
                return;
            }

            _notifier.UnregisterClient(playerId);
            _presencePort.UnsubscribeFromPlayerPresence(playerId);
        }

        private void OnChannelClosed(object sender, EventArgs e)
        {
            Disconnect();
        }

        private void OnChannelFaulted(object sender, EventArgs e)
        {
            Disconnect();
        }
    }
}
