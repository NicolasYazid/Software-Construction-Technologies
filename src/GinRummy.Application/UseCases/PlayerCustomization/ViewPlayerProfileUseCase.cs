using System;

using GinRummy.Application.Ports.PlayerCustomization;
using GinRummy.Domain.Entities.Authentication;
using GinRummy.Domain.Entities.PlayerCareer;
using GinRummy.Domain.Entities.PlayerCustomization;
using GinRummy.Domain.Ports.PlayerCareer;
using GinRummy.Domain.Shared;

namespace GinRummy.Application.UseCases.PlayerCustomization
{
    public class ViewPlayerProfileUseCase : IPlayerProfilePort
    {
        private readonly Func<IUnitOfWork> _unitOfWorkFactory;
        private readonly AccountAccessPolicy _accessPolicy;
        private readonly IRankingRepository _rankingRepository;
        private readonly RankResolver _rankResolver;

        public ViewPlayerProfileUseCase(
            Func<IUnitOfWork> unitOfWorkFactory,
            AccountAccessPolicy accessPolicy,
            IRankingRepository rankingRepository)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _accessPolicy = accessPolicy ?? throw new ArgumentNullException(nameof(accessPolicy));
            _rankingRepository = rankingRepository ?? throw new ArgumentNullException(nameof(rankingRepository));
            _rankResolver = new RankResolver();
        }

        public ViewedPlayerProfile GetPlayerProfile(string accessToken, int publicTag)
        {
            _accessPolicy.IdentifyPlayerByAccessToken(accessToken);
            ViewedPlayerProfile viewedProfile = null;
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                PlayerProfile player = unitOfWork.PlayerAccounts.FindByPublicTag(publicTag);
                if (player != null)
                {
                    PlayerProfile profile = unitOfWork.PlayerAccounts.FindProfile(player.PlayerAccountId);
                    PlayerStats stats = unitOfWork.PlayerAccounts.FindStats(player.PlayerAccountId);
                    Rank rank = _rankResolver.ResolveRank(stats.Score, _rankingRepository.GetAllRanks());
                    viewedProfile = new ViewedPlayerProfile(profile, stats, rank);
                }
            }

            return viewedProfile;
        }
    }
}
