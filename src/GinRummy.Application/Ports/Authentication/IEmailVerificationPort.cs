namespace GinRummy.Application.Ports.Authentication
{
    public interface IEmailVerificationPort
    {
        void VerifyEmail(string email, string verificationCode);

        void ResendVerificationCode(string email);
    }
}
