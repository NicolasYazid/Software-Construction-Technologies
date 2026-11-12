using GinRummy.Domain.Entities.Authentication;

namespace GinRummy.Application.Ports.Authentication
{
    public interface ISignUpPort
    {
        SignUpResult SignUp(SignUpForm form, string localeCode);
    }
}
