using System;
using System.Collections.Generic;

using GinRummy.Domain.Entities.Friendship;
using GinRummy.Domain.Ports.Friendship;

namespace GinRummy.Domain.Tests.Friendship
{
    public class FakeFriendRepository : IFriendRepository
    {
        private readonly List<FriendRequest> _requests;
        private readonly List<FriendShip> _friendShips;

        public FakeFriendRepository()
        {
            _requests = new List<FriendRequest>();
            _friendShips = new List<FriendShip>();
        }

        public FriendRequest FindRequest(int requesterPlayerAccountId, int addresseePlayerAccountId)
        {
            FriendRequest foundRequest = null;
            foreach (FriendRequest request in _requests)
            {
                bool isMatch = (request.RequesterPlayerAccountId == requesterPlayerAccountId)
                    && (request.AddresseePlayerAccountId == addresseePlayerAccountId);
                if (isMatch)
                {
                    foundRequest = request;
                }
            }

            return foundRequest;
        }

        public FriendRequest FindRequestById(int friendRequestId)
        {
            FriendRequest foundRequest = null;
            foreach (FriendRequest request in _requests)
            {
                if (request.FriendRequestId == friendRequestId)
                {
                    foundRequest = request;
                }
            }

            return foundRequest;
        }

        public IList<FriendRequest> GetRequestsReceivedBy(int addresseePlayerAccountId)
        {
            List<FriendRequest> receivedRequests = new List<FriendRequest>();
            foreach (FriendRequest request in _requests)
            {
                if (request.AddresseePlayerAccountId == addresseePlayerAccountId)
                {
                    receivedRequests.Add(request);
                }
            }

            return receivedRequests;
        }

        public int CountRequestsReceivedBy(int addresseePlayerAccountId)
        {
            return GetRequestsReceivedBy(addresseePlayerAccountId).Count;
        }

        public void AddRequest(FriendRequest newRequest)
        {
            if (newRequest == null)
            {
                throw new ArgumentNullException(nameof(newRequest));
            }

            _requests.Add(newRequest);
        }

        public void RemoveRequest(int friendRequestId)
        {
            FriendRequest request = FindRequestById(friendRequestId);
            if (request != null)
            {
                _requests.Remove(request);
            }
        }

        public FriendShip FindFriendShip(int firstPlayerAccountId, int secondPlayerAccountId)
        {
            int lowerPlayerAccountId = Math.Min(firstPlayerAccountId, secondPlayerAccountId);
            int higherPlayerAccountId = Math.Max(firstPlayerAccountId, secondPlayerAccountId);
            FriendShip foundFriendShip = null;
            foreach (FriendShip friendShip in _friendShips)
            {
                bool isMatch = (friendShip.LowerPlayerAccountId == lowerPlayerAccountId)
                    && (friendShip.HigherPlayerAccountId == higherPlayerAccountId);
                if (isMatch)
                {
                    foundFriendShip = friendShip;
                }
            }

            return foundFriendShip;
        }

        public IList<FriendShip> GetFriendShipsOf(int playerAccountId)
        {
            List<FriendShip> playerFriendShips = new List<FriendShip>();
            foreach (FriendShip friendShip in _friendShips)
            {
                bool isPartOfFriendShip = (friendShip.LowerPlayerAccountId == playerAccountId)
                    || (friendShip.HigherPlayerAccountId == playerAccountId);
                if (isPartOfFriendShip)
                {
                    playerFriendShips.Add(friendShip);
                }
            }

            return playerFriendShips;
        }

        public int CountFriendShipsOf(int playerAccountId)
        {
            return GetFriendShipsOf(playerAccountId).Count;
        }

        public void AddFriendShip(FriendShip newFriendShip)
        {
            if (newFriendShip == null)
            {
                throw new ArgumentNullException(nameof(newFriendShip));
            }

            _friendShips.Add(newFriendShip);
        }

        public void RemoveFriendShip(int firstPlayerAccountId, int secondPlayerAccountId)
        {
            FriendShip friendShip = FindFriendShip(firstPlayerAccountId, secondPlayerAccountId);
            if (friendShip != null)
            {
                _friendShips.Remove(friendShip);
            }
        }
    }
}
