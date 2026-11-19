using System;

using GinRummy.Domain;
using GinRummy.Domain.Entities.PlayerCustomization;
using GinRummy.Domain.Shared;

namespace GinRummy.Application.UseCases.DirectDuel
{
    public class DuelPlayerDirectory
    {
        private readonly Func<IUnitOfWork> _unitOfWorkFactory;

        public DuelPlayerDirectory(Func<IUnitOfWork> unitOfWorkFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
        }

        public PendingChallenge CreateChallenge(int challengerId, int challengedPublicTag)
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

            PendingChallenge challenge = null;
            if (challengedProfile != null)
            {
                challenge = new PendingChallenge(ToDuelPlayer(challengerProfile), ToDuelPlayer(challengedProfile));
            }

            return challenge;
        }

        private static DuelPlayer ToDuelPlayer(PlayerProfile profile)
        {
            return new DuelPlayer(profile.PlayerAccountId, profile.Username, profile.PublicTag);
        }
    }
}