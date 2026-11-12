using System;
using System.Collections.Generic;

using GinRummy.Domain;
using GinRummy.Domain.Entities.Authentication;
using GinRummy.Domain.Entities.PlayerCustomization;
using GinRummy.Domain.Entities.PlayerModeration;
using GinRummy.Domain.Shared;

namespace GinRummy.Application.UseCases.DirectDuel
{
    public class DuelPlayerDirectory
    {
        private readonly Func<IUnitOfWork> _unitOfWorkFactory;
        private readonly AccountAccessPolicy _accessPolicy;

        public DuelPlayerDirectory(Func<IUnitOfWork> unitOfWorkFactory, AccountAccessPolicy accessPolicy)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _accessPolicy = accessPolicy ?? throw new ArgumentNullException(nameof(accessPolicy));
        }

        public PendingChallenge CreateChallenge(int challengerId, int challengedPublicTag)
        {
            PlayerProfile challengerProfile;
            PlayerProfile challengedProfile;
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                challengerProfile = unitOfWork.PlayerAccounts.FindProfileByPlayerAccountId(challengerId);
                challengedProfile = unitOfWork.PlayerAccounts.FindByPublicTag(challengedPublicTag);
                EnsureNotSuspended(unitOfWork, challengerId);
            }

            if (challengerProfile == null)
            {
                throw new InvalidAccessTokenException("The access token does not belong to a registered player.");
            }

            if (challengedProfile == null)
            {
                throw new PlayerUnavailableException("No player has that public tag.");
            }

            return new PendingChallenge(ToDuelPlayer(challengerProfile), ToDuelPlayer(challengedProfile));
        }

        public void EnsureCanDuel(int playerId)
        {
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                EnsureNotSuspended(unitOfWork, playerId);
            }
        }

        private void EnsureNotSuspended(IUnitOfWork unitOfWork, int playerId)
        {
            IList<Ban> bans = unitOfWork.Moderation.GetBansOf(playerId);
            _accessPolicy.EnsureNotSuspended(bans);
        }

        private static DuelPlayer ToDuelPlayer(PlayerProfile profile)
        {
            return new DuelPlayer(profile.PlayerAccountId, profile.Username, profile.PublicTag);
        }
    }
}