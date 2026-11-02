using System.Collections.Generic;

namespace GinRummy.Application.Presence
{
    public class ConnectedPlayerRegistry
    {
        private readonly Dictionary<int, ConnectedPlayer> _connectedPlayers;
        private readonly object _registryLock;

        public ConnectedPlayerRegistry()
        {
            _connectedPlayers = new Dictionary<int, ConnectedPlayer>();
            _registryLock = new object();
        }

        public void AddConnectedPlayer(ConnectedPlayer connectedPlayer)
        {
            lock (_registryLock)
            {
                _connectedPlayers[connectedPlayer.Player.PlayerId] = connectedPlayer;
            }
        }

        public ConnectedPlayer RemoveConnectedPlayer(int playerId)
        {
            ConnectedPlayer removedPlayer = null;
            lock (_registryLock)
            {
                if (_connectedPlayers.ContainsKey(playerId))
                {
                    removedPlayer = _connectedPlayers[playerId];
                    _connectedPlayers.Remove(playerId);
                }
            }

            return removedPlayer;
        }

        public ConnectedPlayer FindConnectedPlayer(int playerId)
        {
            ConnectedPlayer foundPlayer = null;
            lock (_registryLock)
            {
                if (_connectedPlayers.ContainsKey(playerId))
                {
                    foundPlayer = _connectedPlayers[playerId];
                }
            }

            return foundPlayer;
        }

        public IList<ConnectedPlayer> GetConnectedPlayers()
        {
            IList<ConnectedPlayer> connectedPlayers;
            lock (_registryLock)
            {
                connectedPlayers = new List<ConnectedPlayer>(_connectedPlayers.Values);
            }

            return connectedPlayers;
        }
    }
}
