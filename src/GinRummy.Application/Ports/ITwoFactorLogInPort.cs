using GinRummy.Domain.Entities.Authentication;

namespace GinRummy.Application.Ports
{
    public interface ITwoFactorLogInPort
    {
        LogInResult VerifyTwoFactorLogIn(string email, string verificationCode, bool keepSignedIn);
    }
}
