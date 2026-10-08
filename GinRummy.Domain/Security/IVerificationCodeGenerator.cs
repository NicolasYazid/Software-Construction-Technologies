namespace GinRummy.Domain.Security
{
    // Contract the game's logic uses to obtain a new verification code.
    // The logic does not need to know how the code is generated.
    public interface IVerificationCodeGenerator
    {
        // Returned as a string so a leading zero is never lost.
        string GenerateCode();
    }
}
