using System;
using System.ServiceModel;

using GinRummy.Application.Ports.Friendship;
using GinRummy.Contracts.DataContracts.Friendship;
using GinRummy.Contracts.Services.Friendship;
using GinRummy.Domain.Entities.Authentication;
using GinRummy.Domain.Entities.Friendship;
using GinRummy.Services.Mappers.Friendship;
using GinRummy.Services.Notifiers;

namespace GinRummy.Services.Services.Friendship
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class FriendshipService : IFriendshipService
    {
        private readonly AccountAccessPolicy _accessPolicy;
        private readonly ISendFriendRequestUseCase _sendFriendRequestUseCase;
        private readonly OnlinePlayers<IFriendshipCallback> _onlinePlayers;
        private int _subscribedPlayerId;
        private IFriendshipCallback _subscribedCallback;

        public FriendshipService(
            AccountAccessPolicy accessPolicy,
            ISendFriendRequestUseCase sendFriendRequestUseCase,
            OnlinePlayers<IFriendshipCallback> onlinePlayers)
        {
            _accessPolicy = accessPolicy ?? throw new ArgumentNullException(nameof(accessPolicy));
            _sendFriendRequestUseCase = sendFriendRequestUseCase ?? throw new ArgumentNullException(nameof(sendFriendRequestUseCase));
            _onlinePlayers = onlinePlayers ?? throw new ArgumentNullException(nameof(onlinePlayers));
        }

        public void SubscribeToFriendship(string accessToken)
        {
            int playerId = _accessPolicy.IdentifyPlayerByAccessToken(accessToken);
            OperationContext context = OperationContext.Current;
            IFriendshipCallback callback = context.GetCallbackChannel<IFriendshipCallback>();
            _onlinePlayers.AddPlayer(playerId, callback);
            _subscribedPlayerId = playerId;
            _subscribedCallback = callback;
            context.Channel.Closed += OnChannelClosed;
            context.Channel.Faulted += OnChannelFaulted;
        }

        public FriendRequestOutcome SendFriendRequest(string accessToken, int addresseePublicTag)
        {
            int senderId = _accessPolicy.IdentifyPlayerByAccessToken(accessToken);
            FriendRequestResult result = _sendFriendRequestUseCase.SendFriendRequest(senderId, addresseePublicTag);

            return FriendshipMapper.MapToFriendRequestOutcome(result);
        }

        public void UnsubscribeFromFriendship()
        {
            LeaveFriendship();
        }

        private void LeaveFriendship()
        {
            IFriendshipCallback callbackToRemove = _subscribedCallback;
            if (callbackToRemove == null)
            {
                return;
            }

            _subscribedCallback = null;
            _onlinePlayers.RemovePlayer(_subscribedPlayerId, callbackToRemove);
        }

        private void OnChannelClosed(object sender, EventArgs e)
        {
            LeaveFriendship();
        }

        private void OnChannelFaulted(object sender, EventArgs e)
        {
            LeaveFriendship();
        }
    }
}
