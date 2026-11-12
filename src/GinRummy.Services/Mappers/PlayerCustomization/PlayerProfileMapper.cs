using System.Collections.Generic;

using GinRummy.Application.UseCases.PlayerCustomization;
using GinRummy.Contracts.DataContracts.PlayerCustomization;
using GinRummy.Domain.Entities.PlayerCareer;
using GinRummy.Domain.Entities.PlayerCustomization;

namespace GinRummy.Services.Mappers.PlayerCustomization
{
    public static class PlayerProfileMapper
    {
        public static PlayerProfileDetails MapToPlayerProfileDetails(ViewedPlayerProfile viewedProfile)
        {
            PlayerProfileDetails details = new PlayerProfileDetails
            {
                Username = viewedProfile.Profile.Username,
                PublicTag = viewedProfile.Profile.PublicTag,
                Bio = viewedProfile.Profile.Bio,
                SocialLinks = MapToSocialLinks(viewedProfile.Profile.SocialMediaLinks),
                MatchesPlayed = viewedProfile.Stats.MatchesPlayed,
                WinRate = viewedProfile.Stats.WinRate,
                Score = viewedProfile.Stats.Score,
                RankName = ResolveRankName(viewedProfile.Rank)
            };

            return details;
        }

        public static SocialLinkDetails MapToSocialLinkDetails(SocialMedia link)
        {
            SocialLinkDetails details = new SocialLinkDetails
            {
                SocialMediaId = link.SocialMediaId,
                PlatformName = link.PlatformName,
                Url = link.Url
            };

            return details;
        }

        private static List<SocialLinkDetails> MapToSocialLinks(IEnumerable<SocialMedia> links)
        {
            List<SocialLinkDetails> socialLinks = new List<SocialLinkDetails>();
            foreach (SocialMedia link in links)
            {
                socialLinks.Add(MapToSocialLinkDetails(link));
            }

            return socialLinks;
        }

        internal static string ResolveRankName(Rank rank)
        {
            string rankName = string.Empty;
            if (rank != null)
            {
                rankName = rank.Name;
            }

            return rankName;
        }
    }
}
