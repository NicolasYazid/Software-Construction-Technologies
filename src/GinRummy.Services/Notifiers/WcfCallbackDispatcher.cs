using System;
using System.Collections.Concurrent;
using System.ServiceModel;
using System.Threading;

using Microsoft.Extensions.Logging;

namespace GinRummy.Services.Notifiers
{
    public class WcfCallbackDispatcher<TCallback> where TCallback : class
    {
        private readonly TCallback _callback;
        private readonly ILogger<WcfCallbackDispatcher<TCallback>> _logger;
        private readonly BlockingCollection<Action<TCallback>> _pendingNotices;

        public WcfCallbackDispatcher(TCallback callback, ILogger<WcfCallbackDispatcher<TCallback>> logger)
        {
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _pendingNotices = new BlockingCollection<Action<TCallback>>();
            Thread senderThread = new Thread(SendPendingNotices);
            senderThread.IsBackground = true;
            senderThread.Start();
        }

        public bool HasCallback(TCallback callback)
        {
            return ReferenceEquals(_callback, callback);
        }

        public void EnqueueNotice(Action<TCallback> notice)
        {
            if (notice == null)
            {
                throw new ArgumentNullException(nameof(notice));
            }

            try
            {
                _pendingNotices.Add(notice);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogDebug(ex, "A notice was discarded because the player already left.");
            }
        }

        public void Stop()
        {
            _pendingNotices.CompleteAdding();
        }

        private void SendPendingNotices()
        {
            bool isReachable = true;
            foreach (Action<TCallback> notice in _pendingNotices.GetConsumingEnumerable())
            {
                if (isReachable)
                {
                    isReachable = TrySendNotice(notice);
                }
            }
        }

        private bool TrySendNotice(Action<TCallback> notice)
        {
            bool isSent = false;
            try
            {
                notice(_callback);
                isSent = true;
            }
            catch (CommunicationException ex)
            {
                _logger.LogWarning(ex, "A notice could not be delivered because the channel of the client failed.");
            }
            catch (TimeoutException ex)
            {
                _logger.LogWarning(ex, "A notice could not be delivered because the client took too long.");
            }
            catch (ObjectDisposedException ex)
            {
                _logger.LogWarning(ex, "A notice could not be delivered because the channel of the client was closed.");
            }

            return isSent;
        }
    }
}
