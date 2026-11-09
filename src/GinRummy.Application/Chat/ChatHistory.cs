using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using GinRummy.Domain.Entities.SocialCommunication;
using GinRummy.Domain.Shared;

namespace GinRummy.Application.Chat
{
    public class ChatHistory
    {
        private const int MaxMessagesPerChannel = 50;
        private readonly IClock _sentAtClock;
        private readonly ConcurrentQueue<SentChatMessage> _lobbyMessages;
        private readonly ConcurrentQueue<SentChatMessage> _matchMessages;

        public ChatHistory(IClock sentAtClock)
        {
            _sentAtClock = sentAtClock ?? throw new ArgumentNullException(nameof(sentAtClock));
            _lobbyMessages = new ConcurrentQueue<SentChatMessage>();
            _matchMessages = new ConcurrentQueue<SentChatMessage>();
        }

        public void AddMessage(SentChatMessage newMessage)
        {
            if (newMessage == null)
            {
                throw new ArgumentNullException(nameof(newMessage));
            }

            newMessage.AssignSentAt(_sentAtClock.UtcNow);
            if (newMessage.Channel == MessageChannel.Match)
            {
                _matchMessages.Enqueue(newMessage);
                RemoveOldestMessages(_matchMessages);
            }
            else
            {
                _lobbyMessages.Enqueue(newMessage);
                RemoveOldestMessages(_lobbyMessages);
            }
        }

        public List<SentChatMessage> GetLobbyHistoryMessages()
        {
            return new List<SentChatMessage>(_lobbyMessages.ToArray());
        }

        public SentChatMessage FindMessageById(Guid messageId)
        {
            SentChatMessage foundMessage = FindMessageInQueue(_lobbyMessages, messageId);
            if (foundMessage == null)
            {
                foundMessage = FindMessageInQueue(_matchMessages, messageId);
            }

            return foundMessage;
        }

        private static void RemoveOldestMessages(ConcurrentQueue<SentChatMessage> messages)
        {
            SentChatMessage removedMessage;
            while (messages.Count > MaxMessagesPerChannel)
            {
                messages.TryDequeue(out removedMessage);
            }
        }

        private static SentChatMessage FindMessageInQueue(ConcurrentQueue<SentChatMessage> messages, Guid messageId)
        {
            SentChatMessage foundMessage = null;
            foreach (SentChatMessage message in messages)
            {
                if (message.MessageId == messageId)
                {
                    foundMessage = message;
                }
            }

            return foundMessage;
        }
    }
}
