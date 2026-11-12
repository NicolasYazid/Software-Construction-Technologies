using System;

using GinRummy.Application.Ports.DirectDuel;
using GinRummy.Application.UseCases.Gameplay;
using GinRummy.Domain;

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

        public void Challenge(int challengerId, int challengedPublicTag)
        {
            PendingChallenge challenge = _playerDirectory.CreateChallenge(challengerId, challengedPublicTag);
            if (!_challengeAnnouncer.IsPlayerOnline(challenge.Challenged.Id))
            {
                throw new PlayerUnavailableException("The challenged player is not connected.");
            }

            _challengeBoard.Post(challenge);
            _challengeAnnouncer.AnnounceChallengeReceived(challenge);
        }

        public void Cancel(int challengerId)
        {
            PendingChallenge cancelledChallenge = _challengeBoard.RemoveFrom(challengerId);
            if (cancelledChallenge != null)
            {
                _challengeAnnouncer.AnnounceChallengeCancelled(cancelledChallenge);
            }
        }

        public void Accept(int challengedId, int challengerPublicTag)
        {
            _playerDirectory.EnsureCanDuel(challengedId);
            ActiveRound startedRound = _challengeBoard.Accept(challengedId, challengerPublicTag);
            _challengeAnnouncer.AnnounceRoundStarted(startedRound);
        }

        public void Decline(int challengedId, int challengerPublicTag)
        {
            PendingChallenge declinedChallenge = _challengeBoard.RemoveAddressedTo(challengedId, challengerPublicTag);
            if (declinedChallenge == null)
            {
                throw new ChallengeNotFoundException("The challenge no longer exists.");
            }

            _challengeAnnouncer.AnnounceChallengeDeclined(declinedChallenge);
        }
    }
}