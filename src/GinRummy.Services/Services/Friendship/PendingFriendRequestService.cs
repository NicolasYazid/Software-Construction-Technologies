using System;
using System.Collections.Generic;
using System.ServiceModel;

using GinRummy.Application.Ports.Friendship;
using GinRummy.Contracts.Services.Friendship;
using GinRummy.Domain.Entities.Authentication;
using GinRummy.Services.Mappers.Friendship;
using ContractFriendRequest = GinRummy.Contracts.DataContracts.Friendship.FriendRequest;
using DomainFriendRequest = GinRummy.Domain.Entities.Friendship.FriendRequest;

namespace GinRummy.Services.Services.Friendship
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class PendingFriendRequestService : IPendingFriendRequestService
    {
        private readonly AccountAccessPolicy _accessPolicy;
        private readonly IViewPendingFriendRequestsUseCase _viewPendingFriendRequestsUseCase;

        public PendingFriendRequestService(
            AccountAccessPolicy accessPolicy,
            IViewPendingFriendRequestsUseCase viewPendingFriendRequestsUseCase)
        {
            _accessPolicy = accessPolicy ?? throw new ArgumentNullException(nameof(accessPolicy));
            _viewPendingFriendRequestsUseCase = viewPendingFriendRequestsUseCase
                ?? throw new ArgumentNullException(nameof(viewPendingFriendRequestsUseCase));
        }

        public List<ContractFriendRequest> GetPendingFriendRequests(string accessToken)
        {
            int addresseeId = _accessPolicy.IdentifyPlayerByAccessToken(accessToken);
            IList<DomainFriendRequest> pendingRequests = _viewPendingFriendRequestsUseCase.GetPendingFriendRequests(addresseeId);

            return FriendshipMapper.MapToFriendRequests(pendingRequests);
        }
    }
}
