namespace GinRummy.Domain.Security
{
    public interface IVerificationCodeGenerator
    {
        // Returned as a string so a leading zero is never lost.
        string GenerateCode();
    }
}
