using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using GinRummy.Application.Ports.PlayerPresence;
using GinRummy.Application.UseCases.PlayerPresence;
using GinRummy.Contracts.Services.PlayerPresence;
using GinRummy.Domain.Entities.Authentication;
using GinRummy.Services.Mappers.PlayerPresence;
using Microsoft.Extensions.Logging;

namespace GinRummy.Services.Notifiers
{
    public class PlayerPresenceAnnouncer : IPlayerPresenceAnnouncer
    {
        private readonly ConcurrentDictionary<AccessTokenOwner, PlayerConnection> _playerConnections;
        private readonly ILogger<PlayerConnection> _connectionLogger;

        public PlayerPresenceAnnouncer(ILogger<PlayerConnection> connectionLogger)
        {
            _connectionLogger = connectionLogger ?? throw new ArgumentNullException(nameof(connectionLogger));
            _playerConnections = new ConcurrentDictionary<AccessTokenOwner, PlayerConnection>();
        }

        public void RegisterPlayerConnection(AccessTokenOwner accessTokenOwner, IPlayerPresenceCallback callback)
        {
            PlayerConnection newConnection = new PlayerConnection(accessTokenOwner, callback, _connectionLogger);
            PlayerConnection replacedConnection;
            _playerConnections.TryRemove(accessTokenOwner, out replacedConnection);
            _playerConnections[accessTokenOwner] = newConnection;

            if (replacedConnection != null)
            {
                replacedConnection.Stop();
            }
        }

        public void UnregisterPlayerConnection(AccessTokenOwner accessTokenOwner)
        {
            PlayerConnection removedConnection;
            _playerConnections.TryRemove(accessTokenOwner, out removedConnection);

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
                onlinePlayers.Add(new OnlinePlayer(playerConnection.AccessTokenOwner, playerConnection.Status));
            }

            return onlinePlayers;
        }

        public void SendCurrentPresenceStatusOfPlayers(AccessTokenOwner playerToNotify, PlayersByStatus lists)
        {
            PlayerConnection playerConnection = FindPlayerConnection(playerToNotify);

            if (playerConnection == null)
            {
                return;
            }

            playerConnection.SendCurrentStatus(PlayerPresenceMapper.MapToPlayerPresenceReport(lists));
        }

        public void SendPlayerPresenceChange(AccessTokenOwner playerToNotify, PlayerPresenceChange change)
        {
            PlayerConnection playerConnection = FindPlayerConnection(playerToNotify);

            if (playerConnection == null)
            {
                return;
            }

            playerConnection.SendPlayerPresence(
                PlayerPresenceMapper.MapToPlayerPresence(change.Player, change.Status, change.IsFriend));
        }

        private PlayerConnection FindPlayerConnection(AccessTokenOwner accessTokenOwner)
        {
            PlayerConnection foundConnection;
            _playerConnections.TryGetValue(accessTokenOwner, out foundConnection);

            return foundConnection;
        }
    }
}
