using GinRummy.Domain.Entities.Players;

namespace GinRummy.Application.Ports
{
    public interface IProfileEditPort
    {
        void SaveProfileChanges(string accessToken, string username, string bio);

        byte[] GetProfilePicture(string accessToken, int publicTag);

        bool ChangeProfilePicture(string accessToken, byte[] picture);

        void RemoveProfilePicture(string accessToken);

        SocialMedia LinkSocialMedia(string accessToken, string platformName, string url);

        void UnlinkSocialMedia(string accessToken, int socialMediaId);

        bool ChangeLanguage(string accessToken, string localeCode);
    }
}
