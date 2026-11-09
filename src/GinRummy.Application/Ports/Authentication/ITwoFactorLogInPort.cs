using GinRummy.Domain.Entities.Authentication;

namespace GinRummy.Application.Ports.Authentication
{
    public interface ITwoFactorLogInPort
    {
        LogInResult VerifyTwoFactorLogIn(string email, string verificationCode, bool keepSignedIn);
    }
}
