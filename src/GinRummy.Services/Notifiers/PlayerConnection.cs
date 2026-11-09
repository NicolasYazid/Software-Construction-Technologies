using System;
using System.Collections.Concurrent;
using System.ServiceModel;
using System.Threading;

using GinRummy.Contracts.Callbacks;
using GinRummy.Contracts.DataContracts;
using GinRummy.Domain.Entities.Authentication;
using GinRummy.Domain.Entities.PlayerPresence;
using Microsoft.Extensions.Logging;

namespace GinRummy.Services.Notifiers
{
    public class PlayerConnection
    {
        private readonly IPlayerPresenceCallback _callback;
        private readonly ILogger<PlayerConnection> _logger;
        private readonly BlockingCollection<Action> _pendingNotices;
        private readonly Thread _senderThread;

        public PlayerConnection(AccessTokenOwner accessTokenOwner, IPlayerPresenceCallback callback, ILogger<PlayerConnection> logger)
        {
            AccessTokenOwner = accessTokenOwner ?? throw new ArgumentNullException(nameof(accessTokenOwner));
            Status = PlayerPresenceStatus.Online;
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _pendingNotices = new BlockingCollection<Action>();
            _senderThread = new Thread(SendPendingNotices);
            _senderThread.IsBackground = true;
            _senderThread.Start();
        }

        public AccessTokenOwner AccessTokenOwner { get; private set; }

        public PlayerPresenceStatus Status { get; private set; }

        public void SendCurrentStatus(PlayerPresenceReport report)
        {
            AddNotice(() => _callback.OnCurrentStatusOfPlayersReported(report));
        }

        public void SendPlayerPresence(PlayerPresence presence)
        {
            AddNotice(() => _callback.OnPlayerPresenceChanged(presence));
        }

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
