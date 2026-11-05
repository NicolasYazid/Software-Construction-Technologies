using GinRummy.Domain.Entities.Authentication;

namespace GinRummy.Application.Ports
{
    public interface ILogInPort
    {
        LogInResult LogIn(string email, string password, bool keepSignedIn);

        GuestAccess LogInAsGuest();
    }
}
