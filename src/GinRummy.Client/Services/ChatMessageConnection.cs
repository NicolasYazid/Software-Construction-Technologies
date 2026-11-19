using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;

using GinRummy.Client.ChatMessageServiceReference;
using Microsoft.Extensions.Logging;

namespace GinRummy.Client.Services
{
    [CallbackBehavior(UseSynchronizationContext = false)]
    public sealed class ChatMessageConnection : IChatMessageServiceCallback
    {
        private readonly Func<InstanceContext, IChatMessageService> _chatServiceFactory;
        private readonly ILogger<ChatMessageConnection> _logger;
        private readonly SynchronizationContext _screenContext;
        private IChatMessageService _chatService;

        public ChatMessageConnection(
            Func<InstanceContext, IChatMessageService> chatServiceFactory,
            ILogger<ChatMessageConnection> logger)
        {
            _chatServiceFactory = chatServiceFactory ?? throw new ArgumentNullException(nameof(chatServiceFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _screenContext = SynchronizationContext.Current ?? new SynchronizationContext();
        }

        public event EventHandler<List<PlayerMessage>> LobbyChatHistoryReceived;
        public event EventHandler<PlayerMessage> ChatMessageReceived;

        public bool Start(string accessToken)
        {
            if (accessToken == null)
            {
                throw new ArgumentNullException(nameof(accessToken));
            }

            bool isSubscribed = false;
            IChatMessageService chatService = _chatServiceFactory(new InstanceContext(this));
            ICommunicationObject communicationObject = (ICommunicationObject)chatService;
            try
            {
                chatService.SubscribeToChat(accessToken);
                communicationObject.Faulted += HandleChannelFaulted;
                _chatService = chatService;
                isSubscribed = true;
            }
            catch (FaultException<ServiceFault> ex)
            {
                communicationObject.Abort();
                _logger.LogWarning(
                    ex,
                    "The server refused to subscribe the player to the chat with the fault {FaultCode}.",
                    ex.Detail.Code);
            }
            catch (FaultException ex)
            {
                communicationObject.Abort();
                _logger.LogError(ex, "The server could not subscribe the player to the chat.");
            }
            catch (CommunicationException ex)
            {
                communicationObject.Abort();
                _logger.LogError(ex, "The chat service could not be reached to subscribe the player.");
            }
            catch (TimeoutException ex)
            {
                communicationObject.Abort();
                _logger.LogError(ex, "The chat service timed out while subscribing the player.");
            }

            return isSubscribed;
        }

        public void SendMessage(string accessToken, MessageToSend message)
        {
            if (accessToken == null)
            {
                throw new ArgumentNullException(nameof(accessToken));
            }

            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (_chatService == null)
            {
                return;
            }

            try
            {
                _chatService.SendMessage(accessToken, message);
            }
            catch (FaultException<ServiceFault> ex)
            {
                _logger.LogWarning(ex, "The server refused the chat message with the fault {FaultCode}.", ex.Detail.Code);
            }
            catch (FaultException ex)
            {
                _logger.LogError(ex, "The server could not send the chat message.");
            }
            catch (CommunicationException ex)
            {
                _logger.LogError(ex, "The chat service could not be reached to send the chat message.");
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "The chat service timed out while sending the chat message.");
            }
        }

        public void Stop()
        {
            if (_chatService == null)
            {
                return;
            }

            IChatMessageService chatService = _chatService;
            _chatService = null;
            ICommunicationObject communicationObject = (ICommunicationObject)chatService;
            try
            {
                chatService.UnsubscribeFromChat();
                communicationObject.Close();
            }
            catch (CommunicationException ex)
            {
                communicationObject.Abort();
                _logger.LogWarning(ex, "The channel of the chat did not close cleanly.");
            }
            catch (TimeoutException ex)
            {
                communicationObject.Abort();
                _logger.LogWarning(ex, "The channel of the chat took too long to close.");
            }
        }

        public void OnLobbyChatHistoryReceived(List<PlayerMessage> messages)
        {
            if (messages == null)
            {
                throw new ArgumentNullException(nameof(messages));
            }

            _screenContext.Post(state => LobbyChatHistoryReceived?.Invoke(this, messages), null);
        }

        public void OnChatMessageReceived(PlayerMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            _screenContext.Post(state => ChatMessageReceived?.Invoke(this, message), null);
        }

        private void HandleChannelFaulted(object sender, EventArgs e)
        {
            _logger.LogWarning("The channel of the chat failed, so the new messages of the lobby no longer arrive.");
        }
    }
}
