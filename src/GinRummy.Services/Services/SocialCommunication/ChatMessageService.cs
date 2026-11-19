using System;
using System.ServiceModel;

using GinRummy.Application.Ports.SocialCommunication;
using GinRummy.Contracts.DataContracts.SocialCommunication;
using GinRummy.Contracts.Services.SocialCommunication;
using GinRummy.Domain.Entities.Authentication;
using GinRummy.Services.Mappers.SocialCommunication;
using GinRummy.Services.Notifiers;

namespace GinRummy.Services.Services.SocialCommunication
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class ChatMessageService : IChatMessageService
    {
        private readonly AccountAccessPolicy _accessPolicy;
        private readonly ISendMessageUseCase _sendMessageUseCase;
        private readonly OnlinePlayers<IChatMessageCallback> _onlinePlayers;
        private int _subscribedPlayerId;
        private IChatMessageCallback _subscribedCallback;

        public ChatMessageService(
            AccountAccessPolicy accessPolicy,
            ISendMessageUseCase sendMessageUseCase,
            OnlinePlayers<IChatMessageCallback> onlinePlayers)
        {
            _accessPolicy = accessPolicy ?? throw new ArgumentNullException(nameof(accessPolicy));
            _sendMessageUseCase = sendMessageUseCase ?? throw new ArgumentNullException(nameof(sendMessageUseCase));
            _onlinePlayers = onlinePlayers ?? throw new ArgumentNullException(nameof(onlinePlayers));
        }

        public void SubscribeToChat(string accessToken)
        {
            int playerId = _accessPolicy.IdentifyPlayerByAccessToken(accessToken);
            OperationContext context = OperationContext.Current;
            IChatMessageCallback callback = context.GetCallbackChannel<IChatMessageCallback>();
            _onlinePlayers.AddPlayer(playerId, callback);
            _subscribedPlayerId = playerId;
            _subscribedCallback = callback;
            context.Channel.Closed += OnChannelClosed;
            context.Channel.Faulted += OnChannelFaulted;
            _sendMessageUseCase.SendLobbyHistoryToPlayer(playerId);
        }

        public void SendMessage(string accessToken, MessageToSend message)
        {
            int authorId = _accessPolicy.IdentifyPlayerByAccessToken(accessToken);
            _sendMessageUseCase.SendMessage(authorId, ChatMessageMapper.MapToSendMessageRequest(message));
        }

        public void UnsubscribeFromChat()
        {
            LeaveChat();
        }

        private void LeaveChat()
        {
            IChatMessageCallback callbackToRemove = _subscribedCallback;
            if (callbackToRemove == null)
            {
                return;
            }

            _subscribedCallback = null;
            _onlinePlayers.RemovePlayer(_subscribedPlayerId, callbackToRemove);
        }

        private void OnChannelClosed(object sender, EventArgs e)
        {
            LeaveChat();
        }

        private void OnChannelFaulted(object sender, EventArgs e)
        {
            LeaveChat();
        }
    }
}
