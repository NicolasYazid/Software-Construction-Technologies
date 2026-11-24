using System;
using System.Collections.Generic;

using GinRummy.Application.UseCases.SocialCommunication;
using GinRummy.Domain.Entities.PlayerCustomization;
using GinRummy.Server.Tests.Application.Friendship;
using NUnit.Framework;

namespace GinRummy.Server.Tests.Application.SocialCommunication
{
    [TestFixture]
    public class ChatHistoryTests
    {
        private const int LobbyMessageLimit = 50;
        private const int MatchMessageLimit = 20;
        private const int FriendMessageLimit = 20;
        private const int FirstGameSessionId = 1;
        private const int SecondGameSessionId = 2;
        private const int AliceId = 1;
        private const int BobId = 2;
        private const int ElenaId = 3;
        private const string AliceUsername = "alice_reyes";
        private const string BobUsername = "bob_carter";
        private const string ElenaUsername = "elena_soto";
        private const string MessageContent = "Hola";

        private static readonly DateTime s_sentAtMoment = new DateTime(2026, 11, 19, 12, 0, 0, DateTimeKind.Utc);

        private ChatHistory _chatHistory;
        private PlayerProfile _alice;
        private PlayerProfile _bob;
        private PlayerProfile _elena;

        [SetUp]
        public void SetUp()
        {
            _chatHistory = new ChatHistory(new FakeClock(s_sentAtMoment));
            _alice = new PlayerProfile(AliceId, AliceUsername);
            _bob = new PlayerProfile(BobId, BobUsername);
            _elena = new PlayerProfile(ElenaId, ElenaUsername);
        }

        [Test]
        public void Test_AddMessage_NullMessage_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _chatHistory.AddMessage(null));
        }

        [Test]
        public void Test_AddMessage_LobbyMessage_AssignsServerTime()
        {
            SentChatMessage lobbyMessage = AddLobbyMessage();

            Assert.AreEqual(s_sentAtMoment, lobbyMessage.SentAt);
        }

        [Test]
        public void Test_GetLobbyHistoryMessages_MoreMessagesThanLimit_KeepsLimit()
        {
            AddLobbyMessages(LobbyMessageLimit + 1);

            List<SentChatMessage> lobbyMessages = _chatHistory.GetLobbyHistoryMessages();

            Assert.AreEqual(LobbyMessageLimit, lobbyMessages.Count);
        }

        [Test]
        public void Test_GetLobbyHistoryMessages_OnlyMatchMessage_ReturnsEmptyList()
        {
            AddMatchMessage(FirstGameSessionId);

            List<SentChatMessage> lobbyMessages = _chatHistory.GetLobbyHistoryMessages();

            Assert.IsEmpty(lobbyMessages);
        }

        [Test]
        public void Test_FindMessageById_OldestLobbyMessageAfterLimit_ReturnsNull()
        {
            SentChatMessage oldestMessage = AddLobbyMessage();
            AddLobbyMessages(LobbyMessageLimit);

            SentChatMessage foundMessage = _chatHistory.FindMessageById(oldestMessage.MessageId);

            Assert.IsNull(foundMessage);
        }

        [Test]
        public void Test_FindMessageById_OldestMatchMessageAfterLimit_ReturnsNull()
        {
            SentChatMessage oldestMessage = AddMatchMessage(FirstGameSessionId);
            AddMatchMessages(FirstGameSessionId, MatchMessageLimit);

            SentChatMessage foundMessage = _chatHistory.FindMessageById(oldestMessage.MessageId);

            Assert.IsNull(foundMessage);
        }

        [Test]
        public void Test_FindMessageById_OtherMatchReachedLimit_ReturnsMessage()
        {
            SentChatMessage matchMessage = AddMatchMessage(SecondGameSessionId);
            AddMatchMessages(FirstGameSessionId, MatchMessageLimit);

            SentChatMessage foundMessage = _chatHistory.FindMessageById(matchMessage.MessageId);

            Assert.IsNotNull(foundMessage);
        }

        [Test]
        public void Test_FindMessageById_PairReachedLimitFromBothSides_ReturnsNull()
        {
            SentChatMessage oldestMessage = AddFriendMessage(_alice, _bob);
            AddFriendMessages(_bob, _alice, FriendMessageLimit);

            SentChatMessage foundMessage = _chatHistory.FindMessageById(oldestMessage.MessageId);

            Assert.IsNull(foundMessage);
        }

        [Test]
        public void Test_FindMessageById_OtherPairReachedLimit_ReturnsMessage()
        {
            SentChatMessage friendMessage = AddFriendMessage(_alice, _bob);
            AddFriendMessages(_alice, _elena, FriendMessageLimit);

            SentChatMessage foundMessage = _chatHistory.FindMessageById(friendMessage.MessageId);

            Assert.IsNotNull(foundMessage);
        }

        [Test]
        public void Test_FindMessageById_MatchMessagesRemoved_ReturnsNull()
        {
            SentChatMessage matchMessage = AddMatchMessage(FirstGameSessionId);
            _chatHistory.RemoveMatchMessages(FirstGameSessionId);

            SentChatMessage foundMessage = _chatHistory.FindMessageById(matchMessage.MessageId);

            Assert.IsNull(foundMessage);
        }

        [Test]
        public void Test_FindMessageById_OtherMatchMessagesRemoved_ReturnsMessage()
        {
            SentChatMessage matchMessage = AddMatchMessage(SecondGameSessionId);
            _chatHistory.RemoveMatchMessages(FirstGameSessionId);

            SentChatMessage foundMessage = _chatHistory.FindMessageById(matchMessage.MessageId);

            Assert.IsNotNull(foundMessage);
        }

        private SentChatMessage AddLobbyMessage()
        {
            SentChatMessage lobbyMessage = SentChatMessage.ForLobby(_alice, MessageContent);
            _chatHistory.AddMessage(lobbyMessage);

            return lobbyMessage;
        }

        private void AddLobbyMessages(int messageCount)
        {
            for (int i = 0; i < messageCount; i++)
            {
                AddLobbyMessage();
            }
        }

        private SentChatMessage AddMatchMessage(int gameSessionId)
        {
            SentChatMessage matchMessage = SentChatMessage.ForMatch(_alice, MessageContent, gameSessionId);
            _chatHistory.AddMessage(matchMessage);

            return matchMessage;
        }

        private void AddMatchMessages(int gameSessionId, int messageCount)
        {
            for (int i = 0; i < messageCount; i++)
            {
                AddMatchMessage(gameSessionId);
            }
        }

        private SentChatMessage AddFriendMessage(PlayerProfile authorProfile, PlayerProfile friendProfile)
        {
            SentChatMessage friendMessage = SentChatMessage.ForFriend(authorProfile, MessageContent, friendProfile);
            _chatHistory.AddMessage(friendMessage);

            return friendMessage;
        }

        private void AddFriendMessages(PlayerProfile authorProfile, PlayerProfile friendProfile, int messageCount)
        {
            for (int i = 0; i < messageCount; i++)
            {
                AddFriendMessage(authorProfile, friendProfile);
            }
        }
    }
}
