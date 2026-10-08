namespace GinRummy.Domain.Security
{
    // Contract the game's logic uses to protect and check player passwords.
    // The logic does not need to know which hashing algorithm is behind it.
    // A concrete adapter provides the implementation.
    public interface IPasswordHasher
    {
        string HashPassword(string plainTextPassword);

        bool VerifyPassword(string plainTextPassword, string hash);
    }
}
