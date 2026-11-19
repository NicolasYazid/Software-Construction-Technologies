using System;
using System.Collections.Generic;
using System.Linq;

using GinRummy.Application.Ports.Gameplay;
using GinRummy.Application.UseCases.DirectDuel;
using GinRummy.Domain;
using GinRummy.Domain.Entities.Gameplay;
using GinRummy.Domain.Entities.PlayerCareer;
using GinRummy.Domain.Entities.PlayerCustomization;
using GinRummy.Domain.Shared;

namespace GinRummy.Application.UseCases.Gameplay
{
    public class MatchRecorder : IMatchRecorder
    {
        private const int SeatsPerMatch = 2;

        private readonly Func<IUnitOfWork> _unitOfWorkFactory;
        private readonly IClock _clock;

        public MatchRecorder(Func<IUnitOfWork> unitOfWorkFactory, IClock clock)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        public void RecordRoundSnapshot(ActiveMatch activeMatch)
        {
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                GameSession gameSession = FindGameSession(unitOfWork, activeMatch);
                RecordScores(gameSession, activeMatch);
                gameSession.GiveTurnTo(FindParticipant(gameSession, activeMatch.Round.NonDealerId).GameParticipantId);
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
            }
        }

        public void RecordEndWithoutWinner(ActiveMatch activeMatch)
        {
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                FindGameSession(unitOfWork, activeMatch).Finish(_clock.UtcNow);
                unitOfWork.Commit();
            }
        }

        public IList<MatchSnapshot> RecoverInterruptedMatches()
        {
            List<MatchSnapshot> snapshots = new List<MatchSnapshot>();
            DateTime endedAt = _clock.UtcNow;
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                foreach (GameSession gameSession in unitOfWork.Games.GetActiveGameSessions())
                {
                    if (IsResumable(gameSession))
                    {
                        snapshots.Add(TakeSnapshot(unitOfWork, gameSession));
                    }
                    else
                    {
                        gameSession.Finish(endedAt);
                    }
                }

                unitOfWork.Commit();
            }

            return snapshots;
        }

        private static GameSession FindGameSession(IUnitOfWork unitOfWork, ActiveMatch activeMatch)
        {
            return unitOfWork.Games.FindActiveGameSessionByPlayerAccountId(activeMatch.Players[0].Id);
        }

        private static GameParticipant FindParticipant(GameSession gameSession, int playerAccountId)
        {
            return gameSession.Participants.First(participant => participant.PlayerAccountId == playerAccountId);
        }

        private static bool IsResumable(GameSession gameSession)
        {
            return gameSession.Participants.Any(participant => participant.GameParticipantId == gameSession.CurrentTurn)
                && (gameSession.Participants.Count == SeatsPerMatch)
                && gameSession.Participants.All(IsResumableSeat);
        }

        private static bool IsResumableSeat(GameParticipant participant)
        {
            return participant.PlayerAccountId.HasValue && (participant.Score < RoundRules.TargetScore);
        }

        private static MatchSnapshot TakeSnapshot(IUnitOfWork unitOfWork, GameSession gameSession)
        {
            GameParticipant nonDealer = gameSession.Participants.First(
                participant => participant.GameParticipantId == gameSession.CurrentTurn);
            GameParticipant dealer = gameSession.Participants.First(participant => participant != nonDealer);
            Dictionary<int, int> scoresByPlayerId = gameSession.Participants.ToDictionary(
                participant => participant.PlayerAccountId.Value,
                participant => participant.Score);

            return new MatchSnapshot(ToDuelPlayer(unitOfWork, dealer), ToDuelPlayer(unitOfWork, nonDealer), scoresByPlayerId);
        }

        private static DuelPlayer ToDuelPlayer(IUnitOfWork unitOfWork, GameParticipant participant)
        {
            PlayerProfile profile = unitOfWork.PlayerAccounts.FindProfileByPlayerAccountId(participant.PlayerAccountId.Value);

            return new DuelPlayer(profile.PlayerAccountId, profile.Username, profile.PublicTag);
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
