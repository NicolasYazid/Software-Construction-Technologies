using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using GinRummy.Application.Ports;
using GinRummy.Application.Presence;
using GinRummy.Contracts.Callbacks;
using GinRummy.Services.Mappers;
using Microsoft.Extensions.Logging;

namespace GinRummy.Services.Notifiers
{
    public class PlayerPresenceAnnouncer : IPlayerPresenceAnnouncer
    {
        private readonly ConcurrentDictionary<int, PlayerConnection> _playerConnections;
        private readonly ILogger<PlayerConnection> _connectionLogger;

        public PlayerPresenceAnnouncer(ILogger<PlayerConnection> connectionLogger)
        {
            _connectionLogger = connectionLogger ?? throw new ArgumentNullException(nameof(connectionLogger));
            _playerConnections = new ConcurrentDictionary<int, PlayerConnection>();
        }

        public void RegisterPlayerConnection(int playerId, IPlayerPresenceCallback callback)
        {
            PlayerConnection newConnection = new PlayerConnection(playerId, callback, _connectionLogger);
            PlayerConnection replacedConnection;
            _playerConnections.TryRemove(playerId, out replacedConnection);
            _playerConnections[playerId] = newConnection;

            if (replacedConnection != null)
            {
                replacedConnection.Stop();
            }
        }

        public void UnregisterPlayerConnection(int playerId)
        {
            PlayerConnection removedConnection;
            _playerConnections.TryRemove(playerId, out removedConnection);

            if (removedConnection != null)
            {
                removedConnection.Stop();
            }
        }

        public IList<OnlinePlayer> GetOnlinePlayers()
        {
            IList<OnlinePlayer> onlinePlayers = new List<OnlinePlayer>();

            foreach (PlayerConnection playerConnection in _playerConnections.Values)
            {
                onlinePlayers.Add(new OnlinePlayer(playerConnection.PlayerId, playerConnection.Status));
            }

            return onlinePlayers;
        }

        public void SendCurrentPresenceStatusOfPlayers(int playerToNotifyId, PlayersByStatus lists)
        {
            PlayerConnection playerConnection = FindPlayerConnection(playerToNotifyId);

            if (playerConnection == null)
            {
                return;
            }

            playerConnection.SendCurrentStatus(PlayerPresenceMapper.MapToPlayerPresenceReport(lists));
        }

        public void SendPlayerPresenceChange(int playerToNotifyId, PlayerPresenceChange change)
        {
            PlayerConnection playerConnection = FindPlayerConnection(playerToNotifyId);

            if (playerConnection == null)
            {
                return;
            }

            playerConnection.SendPlayerPresence(
                PlayerPresenceMapper.MapToPlayerPresence(change.Player, change.Status, change.IsFriend));
        }

        private PlayerConnection FindPlayerConnection(int playerId)
        {
            PlayerConnection foundConnection;
            _playerConnections.TryGetValue(playerId, out foundConnection);

            return foundConnection;
        }
    }
}
