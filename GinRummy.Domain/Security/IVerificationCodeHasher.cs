namespace GinRummy.Domain.Security
{
    public interface IVerificationCodeHasher
    {
        string ComputeHash(string code);

        bool VerifyCode(string code, string hash);
    }
}
