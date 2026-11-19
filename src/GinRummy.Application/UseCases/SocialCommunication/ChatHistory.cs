using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using GinRummy.Domain.Entities.SocialCommunication;
using GinRummy.Domain.Shared;

namespace GinRummy.Application.UseCases.SocialCommunication
{
    public class ChatHistory
    {
        private const int MaxLobbyMessages = 50;
        private const int MaxMatchMessages = 20;
        private const int MaxFriendMessages = 20;
        private const string FriendPairKeySeparator = "-";
        private readonly IClock _sentAtClock;
        private readonly ConcurrentQueue<SentChatMessage> _lobbyMessages;
        private readonly ConcurrentDictionary<int, ConcurrentQueue<SentChatMessage>> _matchMessagesByGameSessionId;
        private readonly ConcurrentDictionary<string, ConcurrentQueue<SentChatMessage>> _friendMessagesByPairKey;

        public ChatHistory(IClock sentAtClock)
        {
            _sentAtClock = sentAtClock ?? throw new ArgumentNullException(nameof(sentAtClock));
            _lobbyMessages = new ConcurrentQueue<SentChatMessage>();
            _matchMessagesByGameSessionId = new ConcurrentDictionary<int, ConcurrentQueue<SentChatMessage>>();
            _friendMessagesByPairKey = new ConcurrentDictionary<string, ConcurrentQueue<SentChatMessage>>();
        }

        public void AddMessage(SentChatMessage newMessage)
        {
            if (newMessage == null)
            {
                throw new ArgumentNullException(nameof(newMessage));
            }

            newMessage.AssignSentAt(_sentAtClock.UtcNow);
            switch (newMessage.Channel)
            {
                case MessageChannel.Match:
                    AddMessageToQueue(GetMatchMessages(newMessage.GameSessionId.Value), newMessage, MaxMatchMessages);
                    break;
                case MessageChannel.Friend:
                    AddMessageToQueue(GetFriendMessages(newMessage), newMessage, MaxFriendMessages);
                    break;
                default:
                    AddMessageToQueue(_lobbyMessages, newMessage, MaxLobbyMessages);
                    break;
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
                foundMessage = FindMessageInQueues(_matchMessagesByGameSessionId.Values, messageId);
            }

            if (foundMessage == null)
            {
                foundMessage = FindMessageInQueues(_friendMessagesByPairKey.Values, messageId);
            }

            return foundMessage;
        }

        public void RemoveMatchMessages(int gameSessionId)
        {
            ConcurrentQueue<SentChatMessage> removedMessages;
            _matchMessagesByGameSessionId.TryRemove(gameSessionId, out removedMessages);
        }

        private static void AddMessageToQueue(ConcurrentQueue<SentChatMessage> messages, SentChatMessage newMessage, int maxMessages)
        {
            messages.Enqueue(newMessage);
            SentChatMessage removedMessage;
            while (messages.Count > maxMessages)
            {
                messages.TryDequeue(out removedMessage);
            }
        }

        private static string BuildFriendPairKey(SentChatMessage friendMessage)
        {
            int authorId = friendMessage.AuthorProfile.PlayerAccountId;
            int addresseeId = friendMessage.AddresseeProfile.PlayerAccountId;

            return Math.Min(authorId, addresseeId) + FriendPairKeySeparator + Math.Max(authorId, addresseeId);
        }

        private static SentChatMessage FindMessageInQueues(ICollection<ConcurrentQueue<SentChatMessage>> messageQueues, Guid messageId)
        {
            SentChatMessage foundMessage = null;
            foreach (ConcurrentQueue<SentChatMessage> messages in messageQueues)
            {
                if (foundMessage == null)
                {
                    foundMessage = FindMessageInQueue(messages, messageId);
                }
            }

            return foundMessage;
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

        private ConcurrentQueue<SentChatMessage> GetMatchMessages(int gameSessionId)
        {
            return _matchMessagesByGameSessionId.GetOrAdd(gameSessionId, new ConcurrentQueue<SentChatMessage>());
        }

        private ConcurrentQueue<SentChatMessage> GetFriendMessages(SentChatMessage friendMessage)
        {
            return _friendMessagesByPairKey.GetOrAdd(BuildFriendPairKey(friendMessage), new ConcurrentQueue<SentChatMessage>());
        }
    }
}
