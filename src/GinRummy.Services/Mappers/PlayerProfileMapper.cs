using System.Collections.Generic;

using GinRummy.Application.UseCases;
using GinRummy.Contracts.DataContracts;
using GinRummy.Domain.Entities.PlayerRanking;
using GinRummy.Domain.Entities.Players;

namespace GinRummy.Services.Mappers
{
    /// <summary>
    /// Turns the profile the use case reads into the one the client shows.
    /// </summary>
    public static class PlayerProfileMapper
    {
        /// <summary>
        /// Maps the profile, the statistics and the rank of a player to the data contract of the profile.
        /// </summary>
        /// <param name="viewedProfile">Profile, statistics and rank of the player.</param>
        /// <returns>The profile ready to be sent.</returns>
        public static PlayerProfileDetails MapToPlayerProfileDetails(ViewedPlayerProfile viewedProfile)
        {
            PlayerProfileDetails details = new PlayerProfileDetails
            {
                Username = viewedProfile.Stats.Player.Username,
                PublicTag = viewedProfile.Stats.Player.PublicTag,
                Bio = viewedProfile.Profile.Bio,
                SocialLinks = MapToSocialLinks(viewedProfile.Profile.SocialMediaLinks),
                MatchesPlayed = viewedProfile.Stats.MatchesPlayed,
                WinRate = viewedProfile.Stats.WinRate,
                Score = viewedProfile.Stats.Score,
                RankName = ResolveRankName(viewedProfile.Rank)
            };

            return details;
        }

        /// <summary>
        /// Maps a social media link to the data contract of the link.
        /// </summary>
        /// <param name="link">Link of a profile.</param>
        /// <returns>The link ready to be sent.</returns>
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

        // The profile and the presence show the same rank, so both send its name the same way.
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
