using System;

using GinRummy.Application.Ports.DirectDuel;
using GinRummy.Application.UseCases.Gameplay;
using GinRummy.Domain;
using GinRummy.Domain.Entities.PlayerCustomization;
using GinRummy.Domain.Shared;

namespace GinRummy.Application.UseCases.DirectDuel
{
    public class ChallengeUseCase : IChallengeUseCase
    {
        private readonly Func<IUnitOfWork> _unitOfWorkFactory;
        private readonly ChallengeBoard _challengeBoard;
        private readonly IChallengeAnnouncer _challengeAnnouncer;

        public ChallengeUseCase(
            Func<IUnitOfWork> unitOfWorkFactory,
            ChallengeBoard challengeBoard,
            IChallengeAnnouncer challengeAnnouncer)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _challengeBoard = challengeBoard ?? throw new ArgumentNullException(nameof(challengeBoard));
            _challengeAnnouncer = challengeAnnouncer ?? throw new ArgumentNullException(nameof(challengeAnnouncer));
        }

        public void Challenge(int challengerId, int challengedPublicTag)
        {
            PendingChallenge challenge = BuildChallenge(challengerId, challengedPublicTag);
            bool isChallengedOnline = _challengeAnnouncer.IsPlayerOnline(challenge.Challenged.Id);
            if (!isChallengedOnline || !_challengeBoard.TryPost(challenge))
            {
                throw new PlayerUnavailableException("The player cannot be challenged right now.");
            }

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
            ActiveRound startedRound = _challengeBoard.Accept(challengedId, challengerPublicTag);
            if (startedRound == null)
            {
                throw new ChallengeNotFoundException("The challenge no longer exists.");
            }

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

        private PendingChallenge BuildChallenge(int challengerId, int challengedPublicTag)
        {
            PlayerProfile challengerProfile;
            PlayerProfile challengedProfile;
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                challengerProfile = unitOfWork.PlayerAccounts.FindProfileByPlayerAccountId(challengerId);
                challengedProfile = unitOfWork.PlayerAccounts.FindByPublicTag(challengedPublicTag);
            }

            if (challengerProfile == null)
            {
                throw new InvalidAccessTokenException("The access token does not belong to a registered player.");
            }

            if (challengedProfile == null)
            {
                throw new PlayerUnavailableException("No player has that public tag.");
            }

            return new PendingChallenge(BuildDuelPlayer(challengerProfile), BuildDuelPlayer(challengedProfile));
        }

        private static DuelPlayer BuildDuelPlayer(PlayerProfile profile)
        {
            return new DuelPlayer(profile.PlayerAccountId, profile.Username, profile.PublicTag);
        }
    }
}
