using System;
using System.Collections.Generic;

using GinRummy.Application.Chat;
using GinRummy.Application.Ports;
using GinRummy.Contracts.Callbacks;
using GinRummy.Contracts.DataContracts;
using GinRummy.Services.Mappers;

namespace GinRummy.Services.Notifiers
{
    public class ChatMessageAnnouncer : IChatMessageAnnouncer
    {
        private readonly OnlinePlayers<IChatMessageCallback> _onlinePlayers;

        public ChatMessageAnnouncer(OnlinePlayers<IChatMessageCallback> onlinePlayers)
        {
            _onlinePlayers = onlinePlayers ?? throw new ArgumentNullException(nameof(onlinePlayers));
        }

        public void AnnounceMessageToAllPlayers(SentChatMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            PlayerMessage playerMessage = ChatMessageMapper.MapToPlayerMessage(message);
            foreach (WcfCallbackDispatcher<IChatMessageCallback> playerDispatcher in _onlinePlayers.GetDispatchersOfAllPlayers())
            {
                playerDispatcher.EnqueueNotice(callback => callback.OnChatMessageReceived(playerMessage));
            }
        }

        public void SendMessageToMatchPlayers(int authorId, int rivalId, SentChatMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            PlayerMessage playerMessage = ChatMessageMapper.MapToPlayerMessage(message);
            EnqueueMessageForPlayer(authorId, playerMessage);
            EnqueueMessageForPlayer(rivalId, playerMessage);
        }

        public void SendLobbyHistoryToPlayer(int playerId, List<SentChatMessage> messages)
        {
            if (messages == null)
            {
                throw new ArgumentNullException(nameof(messages));
            }

            WcfCallbackDispatcher<IChatMessageCallback> playerDispatcher = _onlinePlayers.FindDispatcherOfPlayer(playerId);
            if (playerDispatcher == null)
            {
                return;
            }

            List<PlayerMessage> playerMessages = ChatMessageMapper.MapToPlayerMessages(messages);
            playerDispatcher.EnqueueNotice(callback => callback.OnLobbyChatHistoryReceived(playerMessages));
        }

        private void EnqueueMessageForPlayer(int playerId, PlayerMessage playerMessage)
        {
            WcfCallbackDispatcher<IChatMessageCallback> playerDispatcher = _onlinePlayers.FindDispatcherOfPlayer(playerId);
            if (playerDispatcher != null)
            {
                playerDispatcher.EnqueueNotice(callback => callback.OnChatMessageReceived(playerMessage));
            }
        }
    }
}
