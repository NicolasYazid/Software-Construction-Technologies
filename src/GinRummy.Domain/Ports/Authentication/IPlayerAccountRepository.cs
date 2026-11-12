using System.Collections.Generic;

using GinRummy.Domain.Entities.Authentication;
using GinRummy.Domain.Entities.PlayerCareer;
using GinRummy.Domain.Entities.PlayerCustomization;

namespace GinRummy.Domain.Ports.Authentication
{
    public interface IPlayerAccountRepository
    {
        PlayerAccount FindByEmail(string email);

        PlayerAccount FindByPlayerAccountId(int playerAccountId);

        PlayerProfile FindProfileByPlayerAccountId(int playerAccountId);

        void Add(PlayerAccount newAccount);

        void AddProfile(PlayerProfile newProfile);

        void AddStats(PlayerStats newStats);

        PlayerProfile FindProfile(int playerAccountId);

        PlayerProfile FindByPublicTag(int publicTag);

        IList<PlayerProfile> GetAllPlayers();

        PlayerStats FindStats(int playerAccountId);

        void AddSocialMedia(SocialMedia newLink);

        void RemoveSocialMedia(SocialMedia link);
    }
}
