using System;
using System.Collections.Generic;

using GinRummy.Domain.Entities;
using GinRummy.Domain.Repositories;

namespace GinRummy.Application.Tests
{
    // Test double that stores friendships in memory and leaves the request-related members
    // unused, because the presence coordinator only reads a player's friendships.
    internal class FakeFriendRepository : IFriendRepository
    {
        private readonly Dictionary<int, IList<FriendShip>> _friendShipsByPlayerId;

        public FakeFriendRepository()
        {
            _friendShipsByPlayerId = new Dictionary<int, IList<FriendShip>>();
        }

        public void AddFriendShip(FriendShip newFriendShip)
        {
            Register(newFriendShip.PlayerAId, newFriendShip);
            Register(newFriendShip.PlayerBId, newFriendShip);
        }

        public IList<FriendShip> GetFriendShipsOf(int playerId)
        {
            IList<FriendShip> friendShips = new List<FriendShip>();
            if (_friendShipsByPlayerId.ContainsKey(playerId))
            {
                friendShips = _friendShipsByPlayerId[playerId];
            }

            return friendShips;
        }

        public FriendRequest FindRequest(int requesterPlayerId, int recipientPlayerId)
        {
            throw new NotImplementedException();
        }

        public FriendRequest FindRequestById(int friendRequestId)
        {
            throw new NotImplementedException();
        }

        public IList<FriendRequest> GetRequestsReceivedBy(int recipientPlayerId)
        {
            throw new NotImplementedException();
        }

        public void AddRequest(FriendRequest newRequest)
        {
            throw new NotImplementedException();
        }

        public void RemoveRequest(int friendRequestId)
        {
            throw new NotImplementedException();
        }

        public FriendShip FindFriendShip(int firstPlayerId, int secondPlayerId)
        {
            throw new NotImplementedException();
        }

        public void RemoveFriendShip(int firstPlayerId, int secondPlayerId)
        {
            throw new NotImplementedException();
        }

        private void Register(int playerId, FriendShip friendShip)
        {
            if (!_friendShipsByPlayerId.ContainsKey(playerId))
            {
                _friendShipsByPlayerId[playerId] = new List<FriendShip>();
            }

            _friendShipsByPlayerId[playerId].Add(friendShip);
        }
    }
}
