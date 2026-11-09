using GinRummy.Domain.Entities.Authentication;

namespace GinRummy.Domain.Ports.Authentication
{
    public interface IPasswordHasher
    {
        string HashPassword(Password password);
        bool VerifyPassword(string plainTextPassword, string hash);
    }
}
