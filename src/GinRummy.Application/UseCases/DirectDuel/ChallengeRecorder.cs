using System;
using System.Linq;

using GinRummy.Application.Ports.DirectDuel;
using GinRummy.Domain.Entities.DirectDuel;
using GinRummy.Domain.Entities.Gameplay;
using GinRummy.Domain.Shared;

namespace GinRummy.Application.UseCases.DirectDuel
{
    public class ChallengeRecorder : IChallengeRecorder
    {
        private const int ChallengeLifetimeMinutes = 1;
        private readonly Func<IUnitOfWork> _unitOfWorkFactory;
        private readonly IClock _clock;

        public ChallengeRecorder(Func<IUnitOfWork> unitOfWorkFactory, IClock clock)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        public void RecordSent(PendingChallenge challenge)
        {
            DateTime sentAt = _clock.UtcNow;
            Challenge sentChallenge = new Challenge(
                challenge.Challenger.Id,
                challenge.Challenged.Id,
                new ValidityPeriod(sentAt, sentAt.AddMinutes(ChallengeLifetimeMinutes)));
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                unitOfWork.Challenges.Add(sentChallenge);
                unitOfWork.Commit();
            }
        }

        public void RecordCancelled(PendingChallenge challenge)
        {
            Resolve(challenge, (storedChallenge, resolvedAt) => storedChallenge.Cancel(resolvedAt));
        }

        public void RecordDeclined(PendingChallenge challenge)
        {
            Resolve(challenge, (storedChallenge, resolvedAt) => storedChallenge.Decline(resolvedAt));
        }

        public void RecordExpired(PendingChallenge challenge)
        {
            Resolve(challenge, (storedChallenge, resolvedAt) => storedChallenge.Expire(resolvedAt));
        }

        public bool RecordAccepted(PendingChallenge challenge)
        {
            bool isAccepted;
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                DateTime acceptedAt = _clock.UtcNow;
                Challenge storedChallenge = FindPendingChallenge(unitOfWork, challenge);
                isAccepted = (storedChallenge != null) && !storedChallenge.IsExpiredAt(acceptedAt) ? true : false;
                if (isAccepted)
                {
                    storedChallenge.Accept(StartGameSession(unitOfWork, challenge, acceptedAt), acceptedAt);
                }
                else if (storedChallenge != null)
                {
                    storedChallenge.Expire(acceptedAt);
                }

                unitOfWork.Commit();
            }

            return isAccepted;
        }

        private void Resolve(PendingChallenge challenge, Action<Challenge, DateTime> resolve)
        {
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                Challenge storedChallenge = FindPendingChallenge(unitOfWork, challenge);
                if (storedChallenge != null)
                {
                    resolve(storedChallenge, _clock.UtcNow);
                    unitOfWork.Commit();
                }
            }
        }

        private static int StartGameSession(IUnitOfWork unitOfWork, PendingChallenge challenge, DateTime startedAt)
        {
            GameSession gameSession = new GameSession();
            gameSession.Start(startedAt);
            unitOfWork.Games.Add(gameSession);
            unitOfWork.Games.AddParticipant(GameParticipant.ForPlayer(gameSession.GameSessionId, challenge.Challenger.Id));
            GameParticipant firstToPlay = GameParticipant.ForPlayer(gameSession.GameSessionId, challenge.Challenged.Id);
            unitOfWork.Games.AddParticipant(firstToPlay);
            gameSession.GiveTurnTo(firstToPlay.GameParticipantId);

            return gameSession.GameSessionId;
        }

        private static Challenge FindPendingChallenge(IUnitOfWork unitOfWork, PendingChallenge challenge)
        {
            return unitOfWork.Challenges.GetPendingSentBy(challenge.Challenger.Id)
                .Where(storedChallenge => storedChallenge.ChallengedPlayerAccountId == challenge.Challenged.Id)
                .OrderByDescending(storedChallenge => storedChallenge.ChallengeId)
                .FirstOrDefault();
        }
    }
}
