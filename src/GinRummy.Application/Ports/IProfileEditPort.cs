using GinRummy.Domain.Entities.Players;

namespace GinRummy.Application.Ports
{
    public interface IProfileEditPort
    {
        void SaveProfileChanges(string accessToken, string username, string bio);

        byte[] GetOwnProfilePicture(string accessToken);

        bool ChangeProfilePicture(string accessToken, byte[] picture);

        void RemoveProfilePicture(string accessToken);

        SocialMedia LinkSocialMedia(string accessToken, string platformName, string url);

        void UnlinkSocialMedia(string accessToken, int socialMediaId);
    }
}
