using System;
using System.Collections.Generic;

using GinRummy.Contracts.DataContracts.Friendship;
using GinRummy.Domain.Entities.Friendship;
using GinRummy.Domain.Entities.PlayerCustomization;
using ContractFriendRequest = GinRummy.Contracts.DataContracts.Friendship.FriendRequest;
using DomainFriendRequest = GinRummy.Domain.Entities.Friendship.FriendRequest;

namespace GinRummy.Services.Mappers.Friendship
{
    public static class FriendshipMapper
    {
        public static ContractFriendRequest MapToFriendRequest(DomainFriendRequest newRequest, PlayerProfile requesterProfile)
        {
            if (newRequest == null)
            {
                throw new ArgumentNullException(nameof(newRequest));
            }

            if (requesterProfile == null)
            {
                throw new ArgumentNullException(nameof(requesterProfile));
            }

            return new ContractFriendRequest
            {
                FriendRequestId = newRequest.FriendRequestId,
                RequesterUsername = requesterProfile.Username,
                RequesterPublicTag = requesterProfile.PublicTag,
                SentAt = newRequest.CreatedAt
            };
        }

        public static List<ContractFriendRequest> MapToFriendRequests(IList<DomainFriendRequest> pendingRequests)
        {
            if (pendingRequests == null)
            {
                throw new ArgumentNullException(nameof(pendingRequests));
            }

            List<ContractFriendRequest> mappedRequests = new List<ContractFriendRequest>();
            foreach (DomainFriendRequest pendingRequest in pendingRequests)
            {
                mappedRequests.Add(MapToFriendRequest(pendingRequest, pendingRequest.Requester.PlayerProfile));
            }

            return mappedRequests;
        }

        public static FriendshipEvent MapToReceivedFriendshipEvent(DomainFriendRequest newRequest, PlayerProfile requesterProfile)
        {
            if (newRequest == null)
            {
                throw new ArgumentNullException(nameof(newRequest));
            }

            if (requesterProfile == null)
            {
                throw new ArgumentNullException(nameof(requesterProfile));
            }

            return new FriendshipEvent
            {
                EventType = FriendshipEventType.FriendRequestReceived,
                OtherPlayerPublicTag = requesterProfile.PublicTag,
                FriendRequestId = newRequest.FriendRequestId,
                OtherPlayerUsername = requesterProfile.Username,
                SentAt = newRequest.CreatedAt
            };
        }

        public static FriendshipEvent MapToFriendshipEvent(FriendshipEventType eventType, PlayerProfile otherPlayerProfile)
        {
            if (otherPlayerProfile == null)
            {
                throw new ArgumentNullException(nameof(otherPlayerProfile));
            }

            return new FriendshipEvent
            {
                EventType = eventType,
                OtherPlayerPublicTag = otherPlayerProfile.PublicTag
            };
        }

        public static FriendRequestOutcome MapToFriendRequestOutcome(FriendRequestResult result)
        {
            return (FriendRequestOutcome)Enum.Parse(typeof(FriendRequestOutcome), result.ToString());
        }

        public static FriendRequestResponseOutcome MapToFriendRequestResponseOutcome(FriendRequestResponseResult result)
        {
            return (FriendRequestResponseOutcome)Enum.Parse(typeof(FriendRequestResponseOutcome), result.ToString());
        }
    }
}
