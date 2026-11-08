using System;

using GinRummy.Application.Chat;
using GinRummy.Application.Ports;
using GinRummy.Domain;
using GinRummy.Domain.Entities.Game;
using GinRummy.Domain.Entities.Players;
using GinRummy.Domain.Entities.SocialCommunication;
using GinRummy.Domain.Ports.Repositories;

namespace GinRummy.Application.UseCases
{
    public class SendMessageUseCase : ISendMessageUseCase
    {
        private const int NoRivalPlayerId = 0;
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

        public void SendLobbyMessage(int authorId, string content)
        {
            if (!IsContentValid(content))
            {
                return;
            }

            PlayerProfile authorProfile;
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                authorProfile = FindAuthorProfile(unitOfWork, authorId);
            }

            SentChatMessage lobbyMessage = SentChatMessage.ForLobby(authorProfile, content);
            _chatHistory.AddMessage(lobbyMessage);
            _chatMessageAnnouncer.AnnounceMessageToAllPlayers(lobbyMessage);
        }

        public void SendMatchMessage(int authorId, string content)
        {
            if (!IsContentValid(content))
            {
                return;
            }

            PlayerProfile authorProfile;
            GameSession gameSessionInProgress;
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                authorProfile = FindAuthorProfile(unitOfWork, authorId);
                gameSessionInProgress = unitOfWork.Games.FindGameSessionInProgressByPlayerId(authorId);
            }

            if (gameSessionInProgress == null)
            {
                return;
            }

            int rivalId = FindRivalId(gameSessionInProgress, authorId);
            SentChatMessage matchMessage = SentChatMessage.ForMatch(authorProfile, content, gameSessionInProgress.GameSessionId);
            _chatHistory.AddMessage(matchMessage);
            _chatMessageAnnouncer.SendMessageToMatchPlayers(authorId, rivalId, matchMessage);
        }

        public void SendLobbyHistoryToPlayer(int playerId)
        {
            _chatMessageAnnouncer.SendLobbyHistoryToPlayer(playerId, _chatHistory.GetLobbyHistoryMessages());
        }

        private static bool IsContentValid(string content)
        {
            return !string.IsNullOrWhiteSpace(content) && (content.Length <= Message.ContentMaxLength);
        }

        private static PlayerProfile FindAuthorProfile(IUnitOfWork unitOfWork, int authorId)
        {
            PlayerProfile authorProfile = unitOfWork.PlayerAccounts.FindProfileByPlayerId(authorId);
            if (authorProfile == null)
            {
                throw new InvalidAccessTokenException("The access token does not belong to a registered player.");
            }

            return authorProfile;
        }

        private static int FindRivalId(GameSession gameSessionInProgress, int authorId)
        {
            int rivalId = NoRivalPlayerId;
            foreach (GameParticipant participant in gameSessionInProgress.Participants)
            {
                bool isRegisteredRival = participant.PlayerId.HasValue && (participant.PlayerId.Value != authorId);
                if (isRegisteredRival)
                {
                    rivalId = participant.PlayerId.Value;
                }
            }

            return rivalId;
        }
    }
}
