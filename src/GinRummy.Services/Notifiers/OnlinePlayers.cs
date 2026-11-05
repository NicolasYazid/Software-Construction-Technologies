using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using Microsoft.Extensions.Logging;

namespace GinRummy.Services.Notifiers
{
    public class OnlinePlayers<TCallback> where TCallback : class
    {
        private readonly ConcurrentDictionary<int, WcfCallbackDispatcher<TCallback>> _dispatchersByPlayerId;
        private readonly ILogger<WcfCallbackDispatcher<TCallback>> _dispatcherLogger;

        public OnlinePlayers(ILogger<WcfCallbackDispatcher<TCallback>> dispatcherLogger)
        {
            _dispatcherLogger = dispatcherLogger ?? throw new ArgumentNullException(nameof(dispatcherLogger));
            _dispatchersByPlayerId = new ConcurrentDictionary<int, WcfCallbackDispatcher<TCallback>>();
        }

        public void AddPlayer(int playerId, TCallback callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            WcfCallbackDispatcher<TCallback> newDispatcher = new WcfCallbackDispatcher<TCallback>(callback, _dispatcherLogger);
            WcfCallbackDispatcher<TCallback> replacedDispatcher;
            _dispatchersByPlayerId.TryRemove(playerId, out replacedDispatcher);
            _dispatchersByPlayerId[playerId] = newDispatcher;
            if (replacedDispatcher != null)
            {
                replacedDispatcher.Stop();
            }
        }

        public void RemovePlayer(int playerId, TCallback callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            WcfCallbackDispatcher<TCallback> currentDispatcher;
            bool isFound = _dispatchersByPlayerId.TryGetValue(playerId, out currentDispatcher);
            if (!isFound || !currentDispatcher.HasCallback(callback))
            {
                return;
            }

            _dispatchersByPlayerId.TryRemove(playerId, out _);
            currentDispatcher.Stop();
        }

        public WcfCallbackDispatcher<TCallback> FindDispatcherOfPlayer(int playerId)
        {
            WcfCallbackDispatcher<TCallback> foundDispatcher;
            _dispatchersByPlayerId.TryGetValue(playerId, out foundDispatcher);

            return foundDispatcher;
        }

        public List<WcfCallbackDispatcher<TCallback>> GetDispatchersOfAllPlayers()
        {
            return new List<WcfCallbackDispatcher<TCallback>>(_dispatchersByPlayerId.Values);
        }
    }
}
