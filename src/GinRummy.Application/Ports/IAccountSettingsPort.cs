using GinRummy.Domain.Entities.Authentication;

namespace GinRummy.Application.Ports
{
    public interface IAccountSettingsPort
    {
        PlayerAccount GetAccountSettings(string accessToken);

        void RequestTwoFactorActivation(string accessToken);

        void DisableTwoFactor(string accessToken, string password);

        void RequestPasswordChange(string accessToken, string currentPassword, string newPassword);

        void RequestEmailChange(string accessToken, string newEmail);

        void ChangeChatFilter(string accessToken, bool isChatFilterEnabled);
    }
}
