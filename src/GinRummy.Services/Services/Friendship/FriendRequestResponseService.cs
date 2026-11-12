using System;
using System.ServiceModel;

using GinRummy.Application.Ports.Friendship;
using GinRummy.Contracts.DataContracts.Friendship;
using GinRummy.Contracts.Services.Friendship;
using GinRummy.Domain.Entities.Authentication;
using GinRummy.Domain.Entities.Friendship;
using GinRummy.Services.Mappers.Friendship;

namespace GinRummy.Services.Services.Friendship
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class FriendRequestResponseService : IFriendRequestResponseService
    {
        private readonly AccountAccessPolicy _accessPolicy;
        private readonly IAcceptFriendRequestUseCase _acceptFriendRequestUseCase;
        private readonly IDeclineFriendRequestUseCase _declineFriendRequestUseCase;

        public FriendRequestResponseService(
            AccountAccessPolicy accessPolicy,
            IAcceptFriendRequestUseCase acceptFriendRequestUseCase,
            IDeclineFriendRequestUseCase declineFriendRequestUseCase)
        {
            _accessPolicy = accessPolicy ?? throw new ArgumentNullException(nameof(accessPolicy));
            _acceptFriendRequestUseCase = acceptFriendRequestUseCase ?? throw new ArgumentNullException(nameof(acceptFriendRequestUseCase));
            _declineFriendRequestUseCase = declineFriendRequestUseCase
                ?? throw new ArgumentNullException(nameof(declineFriendRequestUseCase));
        }

        public FriendRequestResponseOutcome AcceptFriendRequest(string accessToken, int friendRequestId)
        {
            int addresseeId = _accessPolicy.IdentifyPlayerByAccessToken(accessToken);
            FriendRequestResponseResult result = _acceptFriendRequestUseCase.AcceptFriendRequest(addresseeId, friendRequestId);

            return FriendshipMapper.MapToFriendRequestResponseOutcome(result);
        }

        public FriendRequestResponseOutcome DeclineFriendRequest(string accessToken, int friendRequestId)
        {
            int addresseeId = _accessPolicy.IdentifyPlayerByAccessToken(accessToken);
            FriendRequestResponseResult result = _declineFriendRequestUseCase.DeclineFriendRequest(addresseeId, friendRequestId);

            return FriendshipMapper.MapToFriendRequestResponseOutcome(result);
        }
    }
}
