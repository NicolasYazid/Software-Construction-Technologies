namespace GinRummy.Domain.Entities.AccountProtection
{
    public enum VerificationCodeCheckResult
    {
        Valid,

        InvalidFormat,

        AlreadyUsed,

        Expired,

        TooManyAttempts,

        Incorrect
    }
}
