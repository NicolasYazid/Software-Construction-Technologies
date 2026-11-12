using System;

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

        public static FriendRequestOutcome MapToFriendRequestOutcome(FriendRequestResult result)
        {
            return (FriendRequestOutcome)Enum.Parse(typeof(FriendRequestOutcome), result.ToString());
        }
    }
}
