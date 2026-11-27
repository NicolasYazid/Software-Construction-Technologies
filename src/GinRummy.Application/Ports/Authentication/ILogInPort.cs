using GinRummy.Domain.Entities.Authentication;

namespace GinRummy.Application.Ports.Authentication
{
    public interface ILogInPort
    {
        LogInResult LogIn(string email, string password, bool isKeepSignedIn);

        GuestAccess LogInAsGuest();
    }
}
