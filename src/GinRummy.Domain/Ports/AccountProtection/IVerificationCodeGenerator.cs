namespace GinRummy.Domain.Ports.AccountProtection
{
    public interface IVerificationCodeGenerator
    {
        string GenerateCode();
    }
}
