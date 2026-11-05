namespace GinRummy.Application.Ports
{
    public interface IAccountChangeConfirmationPort
    {
        void ConfirmTwoFactorActivation(string accessToken, string verificationCode);

        void ConfirmPasswordChange(string accessToken, string newPassword, string verificationCode);

        void ConfirmEmailChange(string accessToken, string newEmail, string verificationCode);
    }
}
