using System;

using GinRummy.Application.Ports.DirectDuel;
using GinRummy.Application.UseCases.Gameplay;

namespace GinRummy.Application.UseCases.DirectDuel
{
    public class ChallengeUseCase : IChallengeUseCase
    {
        private readonly DuelPlayerDirectory _playerDirectory;
        private readonly ChallengeBoard _challengeBoard;
        private readonly IChallengeAnnouncer _challengeAnnouncer;

        public ChallengeUseCase(
            DuelPlayerDirectory playerDirectory,
            ChallengeBoard challengeBoard,
            IChallengeAnnouncer challengeAnnouncer)
        {
            _playerDirectory = playerDirectory ?? throw new ArgumentNullException(nameof(playerDirectory));
            _challengeBoard = challengeBoard ?? throw new ArgumentNullException(nameof(challengeBoard));
            _challengeAnnouncer = challengeAnnouncer ?? throw new ArgumentNullException(nameof(challengeAnnouncer));
        }

        public ChallengeOutcome Challenge(int challengerId, int challengedPublicTag)
        {
            ChallengeOutcome outcome = ChallengeOutcome.AccountSuspended;
            if (_playerDirectory.CanDuel(challengerId))
            {
                outcome = Post(_playerDirectory.CreateChallenge(challengerId, challengedPublicTag));
            }

            return outcome;
        }

        public void Cancel(int challengerId)
        {
            PendingChallenge cancelledChallenge = _challengeBoard.RemoveFrom(challengerId);
            if (cancelledChallenge != null)
            {
                _challengeAnnouncer.AnnounceChallengeCancelled(cancelledChallenge);
            }
        }

        public ChallengeOutcome Accept(int challengedId, int challengerPublicTag)
        {
            ChallengeOutcome outcome = ChallengeOutcome.AccountSuspended;
            ActiveRound startedRound = null;
            if (_playerDirectory.CanDuel(challengedId))
            {
                outcome = _challengeBoard.Accept(challengedId, challengerPublicTag, out startedRound);
            }

            if (outcome == ChallengeOutcome.Done)
            {
                _challengeAnnouncer.AnnounceRoundStarted(startedRound);
            }

            return outcome;
        }

        public ChallengeOutcome Decline(int challengedId, int challengerPublicTag)
        {
            ChallengeOutcome outcome = ChallengeOutcome.ChallengeExpired;
            PendingChallenge declinedChallenge = _challengeBoard.RemoveAddressedTo(challengedId, challengerPublicTag);
            if (declinedChallenge != null)
            {
                _challengeAnnouncer.AnnounceChallengeDeclined(declinedChallenge);
                outcome = ChallengeOutcome.Done;
            }

            return outcome;
        }

        private ChallengeOutcome Post(PendingChallenge challenge)
        {
            ChallengeOutcome outcome;
            if (challenge == null)
            {
                outcome = ChallengeOutcome.PlayerNotFound;
            }
            else if (!_challengeAnnouncer.IsPlayerOnline(challenge.Challenged.Id))
            {
                outcome = ChallengeOutcome.PlayerNotAvailable;
            }
            else
            {
                outcome = _challengeBoard.Post(challenge);
            }

            if (outcome == ChallengeOutcome.Done)
            {
                _challengeAnnouncer.AnnounceChallengeReceived(challenge);
            }

            return outcome;
        }
    }
}