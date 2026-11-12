using System;

using GinRummy.Domain.Ports.Friendship;

namespace GinRummy.Domain.Entities.Friendship
{
    public static class FriendRequestRules
    {
        public const int MaxFriendsPerPlayer = 30;
        public const int MaxPendingRequestsPerPlayer = 30;
        public const int MaxRequestsPerPeriod = 6;
        public static readonly TimeSpan RequestCountingPeriod = TimeSpan.FromMinutes(1);
        public static readonly TimeSpan WaitingTimeAfterRejectionOrRemoval = TimeSpan.FromMinutes(2);

        public static FriendRequestResult CheckNewRequest(IFriendRepository friendRepository, int senderId, int addresseeId)
        {
            if (friendRepository == null)
            {
                throw new ArgumentNullException(nameof(friendRepository));
            }

            FriendRequestResult result = FriendRequestResult.Sent;
            if (senderId == addresseeId)
            {
                result = FriendRequestResult.SelfRequest;
            }
            else if (friendRepository.FindFriendShip(senderId, addresseeId) != null)
            {
                result = FriendRequestResult.AlreadyFriends;
            }
            else if (friendRepository.FindRequest(senderId, addresseeId) != null)
            {
                result = FriendRequestResult.RequestAlreadySent;
            }
            else if (friendRepository.FindRequest(addresseeId, senderId) != null)
            {
                result = FriendRequestResult.IncomingRequestExists;
            }
            else if (friendRepository.CountFriendShipsOf(senderId) >= MaxFriendsPerPlayer)
            {
                result = FriendRequestResult.SenderFriendLimitReached;
            }
            else if (friendRepository.CountFriendShipsOf(addresseeId) >= MaxFriendsPerPlayer)
            {
                result = FriendRequestResult.AddresseeFriendLimitReached;
            }
            else if (friendRepository.CountRequestsReceivedBy(addresseeId) >= MaxPendingRequestsPerPlayer)
            {
                result = FriendRequestResult.AddresseeHasTooManyRequests;
            }

            return result;
        }

        public static FriendRequestResponseResult CheckAcceptance(IFriendRepository friendRepository, int requesterId, int addresseeId)
        {
            if (friendRepository == null)
            {
                throw new ArgumentNullException(nameof(friendRepository));
            }

            FriendRequestResponseResult result = FriendRequestResponseResult.Accepted;
            bool areAlreadyFriends = friendRepository.FindFriendShip(requesterId, addresseeId) != null;
            if (!areAlreadyFriends && (friendRepository.CountFriendShipsOf(requesterId) >= MaxFriendsPerPlayer))
            {
                result = FriendRequestResponseResult.RequesterFriendLimitReached;
            }
            else if (!areAlreadyFriends && (friendRepository.CountFriendShipsOf(addresseeId) >= MaxFriendsPerPlayer))
            {
                result = FriendRequestResponseResult.AddresseeFriendLimitReached;
            }

            return result;
        }
    }
}
