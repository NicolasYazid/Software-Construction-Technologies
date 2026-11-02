using System;
using System.Collections.Concurrent;
using System.ServiceModel;
using System.Threading;

using GinRummy.Contracts.Callbacks;
using GinRummy.Contracts.DataContracts;
using Microsoft.Extensions.Logging;

namespace GinRummy.Services.Notifiers
{
    /// <summary>
    /// A client that subscribed to the presence of the players: its callback channel and the queue of notices waiting
    /// to be sent through it. Each client sends from its own queue, so a slow or dead client never holds back the
    /// notices of the others.
    /// </summary>
    public class SubscribedClient
    {
        private readonly IPlayerPresenceCallback _callback;
        private readonly ILogger<SubscribedClient> _logger;
        private readonly BlockingCollection<Action> _pendingNotices;
        private readonly Thread _senderThread;

        /// <summary>
        /// Builds the client and starts the thread that sends its notices.
        /// </summary>
        /// <param name="callback">Callback channel of the client.</param>
        /// <param name="logger">Logger of the server.</param>
        /// <exception cref="ArgumentNullException">Thrown when an argument is null.</exception>
        public SubscribedClient(IPlayerPresenceCallback callback, ILogger<SubscribedClient> logger)
        {
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _pendingNotices = new BlockingCollection<Action>();
            _senderThread = new Thread(SendPendingNotices);
            _senderThread.IsBackground = true;
            _senderThread.Start();
        }

        /// <summary>
        /// Queues the report of the current status of the players.
        /// </summary>
        /// <param name="report">Report to send to the client.</param>
        public void SendCurrentStatus(PlayerPresenceReport report)
        {
            AddNotice(() => _callback.OnCurrentStatusOfPlayersReported(report));
        }

        /// <summary>
        /// Queues the notice that the status of one player changed.
        /// </summary>
        /// <param name="presence">Player whose status changed.</param>
        public void SendPlayerPresence(PlayerPresence presence)
        {
            AddNotice(() => _callback.OnPlayerPresenceChanged(presence));
        }

        /// <summary>
        /// Stops accepting notices. The ones already queued are still sent, and then the sender thread ends.
        /// </summary>
        public void Stop()
        {
            _pendingNotices.CompleteAdding();
        }

        private void AddNotice(Action notice)
        {
            try
            {
                _pendingNotices.Add(notice);
            }
            catch (InvalidOperationException)
            {
                // The client was stopped while this notice was being added, so nobody is waiting for it anymore.
            }
        }

        private void SendPendingNotices()
        {
            bool isReachable = true;
            foreach (Action notice in _pendingNotices.GetConsumingEnumerable())
            {
                if (isReachable)
                {
                    isReachable = TrySend(notice);
                }
            }
        }

        // A client that cannot be reached keeps no more notices. Its channel closing or failing is what removes it from the server.
        private bool TrySend(Action notice)
        {
            bool isSent = false;
            try
            {
                notice();
                isSent = true;
            }
            catch (CommunicationException ex)
            {
                _logger.LogWarning(ex, "A presence notice could not be delivered because the channel of the client failed.");
            }
            catch (TimeoutException ex)
            {
                _logger.LogWarning(ex, "A presence notice could not be delivered because the client took too long.");
            }
            catch (ObjectDisposedException ex)
            {
                _logger.LogWarning(ex, "A presence notice could not be delivered because the channel of the client was closed.");
            }

            return isSent;
        }
    }
}
