using System;

using GinRummy.Application.Ports.Gameplay;
using GinRummy.Application.UseCases.SocialCommunication;
using GinRummy.Domain.Entities.Gameplay;
using GinRummy.Domain.Entities.PlayerCareer;
using GinRummy.Domain.Shared;

namespace GinRummy.Application.UseCases.Gameplay
{
    public class MatchRecorder : IMatchRecorder
    {
        private readonly Func<IUnitOfWork> _unitOfWorkFactory;
        private readonly IClock _clock;
        private readonly ChatHistory _chatHistory;

        public MatchRecorder(Func<IUnitOfWork> unitOfWorkFactory, IClock clock, ChatHistory chatHistory)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _chatHistory = chatHistory ?? throw new ArgumentNullException(nameof(chatHistory));
        }

        public void RecordRoundScores(ActiveMatch activeMatch)
        {
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                RecordScores(FindGameSession(unitOfWork, activeMatch), activeMatch);
                unitOfWork.Commit();
            }
        }

        public void RecordEnd(ActiveMatch activeMatch, int winnerId)
        {
            int loserId = activeMatch.FindOpponent(winnerId).Id;
            DateTime endedAt = _clock.UtcNow;
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                GameSession gameSession = FindGameSession(unitOfWork, activeMatch);
                RecordScores(gameSession, activeMatch);
                gameSession.Finish(endedAt);
                unitOfWork.Games.AddHistory(new GameHistory(winnerId, loserId, true, PlayerStats.PointsPerMatch, endedAt));
                unitOfWork.Games.AddHistory(new GameHistory(loserId, winnerId, false, PlayerStats.PointsPerMatch, endedAt));
                unitOfWork.PlayerAccounts.FindStats(winnerId).RecordWin();
                unitOfWork.PlayerAccounts.FindStats(loserId).RecordLoss();
                unitOfWork.Commit();
                _chatHistory.RemoveMatchMessages(gameSession.GameSessionId);
            }
        }

        public void RecordEndWithoutWinner(ActiveMatch activeMatch)
        {
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                GameSession gameSession = FindGameSession(unitOfWork, activeMatch);
                gameSession.Finish(_clock.UtcNow);
                unitOfWork.Commit();
                _chatHistory.RemoveMatchMessages(gameSession.GameSessionId);
            }
        }

        public void FinishInterruptedMatches()
        {
            DateTime endedAt = _clock.UtcNow;
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                foreach (GameSession gameSession in unitOfWork.Games.GetActiveGameSessions())
                {
                    gameSession.Finish(endedAt);
                }

                unitOfWork.Commit();
            }
        }

        private static GameSession FindGameSession(IUnitOfWork unitOfWork, ActiveMatch activeMatch)
        {
            return unitOfWork.Games.FindActiveGameSessionByPlayerAccountId(activeMatch.Players[0].Id);
        }

        private static void RecordScores(GameSession gameSession, ActiveMatch activeMatch)
        {
            foreach (GameParticipant participant in gameSession.Participants)
            {
                participant.RecordScore(activeMatch.Match.GetScore(participant.PlayerAccountId.Value));
            }
        }
    }
}
