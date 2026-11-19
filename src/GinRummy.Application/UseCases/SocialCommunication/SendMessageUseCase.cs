using System;

using GinRummy.Application.Ports.SocialCommunication;
using GinRummy.Domain;
using GinRummy.Domain.Entities.Gameplay;
using GinRummy.Domain.Entities.PlayerCustomization;
using GinRummy.Domain.Entities.SocialCommunication;
using GinRummy.Domain.Shared;

namespace GinRummy.Application.UseCases.SocialCommunication
{
    public class SendMessageUseCase : ISendMessageUseCase
    {
        private const int NoRivalPlayerAccountId = 0;
        private readonly Func<IUnitOfWork> _unitOfWorkFactory;
        private readonly ChatHistory _chatHistory;
        private readonly IChatMessageAnnouncer _chatMessageAnnouncer;

        public SendMessageUseCase(
            Func<IUnitOfWork> unitOfWorkFactory,
            ChatHistory chatHistory,
            IChatMessageAnnouncer chatMessageAnnouncer)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _chatHistory = chatHistory ?? throw new ArgumentNullException(nameof(chatHistory));
            _chatMessageAnnouncer = chatMessageAnnouncer ?? throw new ArgumentNullException(nameof(chatMessageAnnouncer));
        }

        public void SendMessage(int authorId, SendMessageRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (!IsContentValid(request.Content))
            {
                return;
            }

            switch (request.Channel)
            {
                case MessageChannel.Lobby:
                    SendLobbyMessage(authorId, request.Content);
                    break;
                case MessageChannel.Match:
                    SendMatchMessage(authorId, request.Content);
                    break;
                case MessageChannel.Friend:
                    SendFriendMessage(authorId, request.Content, request.FriendPublicTag);
                    break;
                default:
                    break;
            }
        }

        public void SendLobbyHistoryToPlayer(int playerId)
        {
            _chatMessageAnnouncer.SendLobbyHistoryToPlayer(playerId, _chatHistory.GetLobbyHistoryMessages());
        }

        private void SendLobbyMessage(int authorId, string content)
        {
            PlayerProfile authorProfile;
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                authorProfile = FindAuthorProfile(unitOfWork, authorId);
            }

            SentChatMessage lobbyMessage = SentChatMessage.ForLobby(authorProfile, content);
            _chatHistory.AddMessage(lobbyMessage);
            _chatMessageAnnouncer.AnnounceMessageToAllPlayers(lobbyMessage);
        }

        private void SendMatchMessage(int authorId, string content)
        {
            PlayerProfile authorProfile;
            GameSession gameSessionInProgress;
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                authorProfile = FindAuthorProfile(unitOfWork, authorId);
                gameSessionInProgress = unitOfWork.Games.FindGameSessionInProgressByPlayerAccountId(authorId);
            }

            if (gameSessionInProgress == null)
            {
                return;
            }

            int rivalId = FindRivalId(gameSessionInProgress, authorId);
            SentChatMessage matchMessage = SentChatMessage.ForMatch(authorProfile, content, gameSessionInProgress.GameSessionId);
            _chatHistory.AddMessage(matchMessage);
            _chatMessageAnnouncer.SendMessageToAuthorAndAddressee(authorId, rivalId, matchMessage);
        }

        private void SendFriendMessage(int authorId, string content, int friendPublicTag)
        {
            PlayerProfile authorProfile;
            PlayerProfile friendProfile;
            bool hasFriendShip = false;
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                authorProfile = FindAuthorProfile(unitOfWork, authorId);
                friendProfile = unitOfWork.PlayerAccounts.FindByPublicTag(friendPublicTag);
                if (friendProfile != null)
                {
                    hasFriendShip = unitOfWork.Friends.FindFriendShip(authorId, friendProfile.PlayerAccountId) != null ? true : false;
                }
            }

            bool canReceiveMessage = hasFriendShip && _chatMessageAnnouncer.IsPlayerOnline(friendProfile.PlayerAccountId) ? true : false;
            if (!canReceiveMessage)
            {
                return;
            }

            SentChatMessage friendMessage = SentChatMessage.ForFriend(authorProfile, content, friendProfile);
            _chatHistory.AddMessage(friendMessage);
            _chatMessageAnnouncer.SendMessageToAuthorAndAddressee(authorId, friendProfile.PlayerAccountId, friendMessage);
        }

        private static bool IsContentValid(string content)
        {
            return !string.IsNullOrWhiteSpace(content) && (content.Length <= Message.ContentMaxLength) ? true : false;
        }

        private static PlayerProfile FindAuthorProfile(IUnitOfWork unitOfWork, int authorId)
        {
            PlayerProfile authorProfile = unitOfWork.PlayerAccounts.FindProfileByPlayerAccountId(authorId);
            if (authorProfile == null)
            {
                throw new InvalidAccessTokenException("The access token does not belong to a registered player.");
            }

            return authorProfile;
        }

        private static int FindRivalId(GameSession gameSessionInProgress, int authorId)
        {
            int rivalId = NoRivalPlayerAccountId;
            foreach (GameParticipant participant in gameSessionInProgress.Participants)
            {
                bool isRegisteredRival = participant.PlayerAccountId.HasValue && (participant.PlayerAccountId.Value != authorId)
                    ? true
                    : false;
                if (isRegisteredRival)
                {
                    rivalId = participant.PlayerAccountId.Value;
                }
            }

            return rivalId;
        }
    }
}
