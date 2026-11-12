namespace GinRummy.Domain.Ports.AccountProtection
{
    public interface IVerificationCodeHasher
    {
        string ComputeHash(string code);

        bool VerifyCode(string code, string hash);
    }
}
