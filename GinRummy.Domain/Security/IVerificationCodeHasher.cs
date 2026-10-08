namespace GinRummy.Domain.Security
{
    // Contract the game's logic uses to hash and check verification codes.
    // The logic does not need to know which algorithm is behind it.
    public interface IVerificationCodeHasher
    {
        string ComputeHash(string code);

        bool VerifyCode(string code, string hash);
    }
}
