using System;
using System.ServiceModel;

using GinRummy.Application.Ports.Friendship;
using GinRummy.Contracts.DataContracts.Friendship;
using GinRummy.Contracts.Services.Friendship;
using GinRummy.Domain.Entities.Friendship;
using GinRummy.Services.Mappers.Friendship;

namespace GinRummy.Services.Services.Friendship
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class FriendshipService : IFriendshipService
    {
        private readonly FriendshipServiceTools _serviceTools;
        private readonly ISendFriendRequestUseCase _sendFriendRequestUseCase;
        private readonly IRemoveFriendUseCase _removeFriendUseCase;
        private int _subscribedPlayerId;
        private IFriendshipCallback _subscribedCallback;

        public FriendshipService(
            FriendshipServiceTools serviceTools,
            ISendFriendRequestUseCase sendFriendRequestUseCase,
            IRemoveFriendUseCase removeFriendUseCase)
        {
            _serviceTools = serviceTools ?? throw new ArgumentNullException(nameof(serviceTools));
            _sendFriendRequestUseCase = sendFriendRequestUseCase ?? throw new ArgumentNullException(nameof(sendFriendRequestUseCase));
            _removeFriendUseCase = removeFriendUseCase ?? throw new ArgumentNullException(nameof(removeFriendUseCase));
        }

        public void SubscribeToFriendship(string accessToken)
        {
            int playerId = _serviceTools.AccessPolicy.IdentifyPlayerByAccessToken(accessToken);
            OperationContext context = OperationContext.Current;
            IFriendshipCallback callback = context.GetCallbackChannel<IFriendshipCallback>();
            _serviceTools.OnlinePlayers.AddPlayer(playerId, callback);
            _subscribedPlayerId = playerId;
            _subscribedCallback = callback;
            context.Channel.Closed += HandleChannelClosed;
            context.Channel.Faulted += HandleChannelFaulted;
        }

        public FriendRequestOutcome SendFriendRequest(string accessToken, int addresseePublicTag)
        {
            int senderId = _serviceTools.AccessPolicy.IdentifyPlayerByAccessToken(accessToken);
            FriendRequestResult result = _sendFriendRequestUseCase.SendFriendRequest(senderId, addresseePublicTag);

            return FriendshipMapper.MapToFriendRequestOutcome(result);
        }

        public void RemoveFriend(string accessToken, int friendPublicTag)
        {
            int playerId = _serviceTools.AccessPolicy.IdentifyPlayerByAccessToken(accessToken);
            _removeFriendUseCase.RemoveFriend(playerId, friendPublicTag);
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
            _serviceTools.OnlinePlayers.RemovePlayer(_subscribedPlayerId, callbackToRemove);
        }

        private void HandleChannelClosed(object sender, EventArgs e)
        {
            LeaveFriendship();
        }

        private void HandleChannelFaulted(object sender, EventArgs e)
        {
            LeaveFriendship();
        }
    }
}
