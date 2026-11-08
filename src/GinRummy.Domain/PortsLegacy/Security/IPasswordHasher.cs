using GinRummy.Domain.Security;

namespace GinRummy.Domain.Ports.Security
{
    public interface IPasswordHasher
    {
        string HashPassword(Password password);
        bool VerifyPassword(string plainTextPassword, string hash);
    }
}
