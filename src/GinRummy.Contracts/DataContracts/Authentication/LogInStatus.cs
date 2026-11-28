namespace GinRummy.Contracts.DataContracts.Authentication
{
    public enum LogInStatus
    {
        Accepted,
        RequiredField,
        DisallowedCharacters,
        InvalidCredentials,
        EmailNotVerified,
        AccountBanned,
        ResendLimitReached,
        CodeInvalidFormat,
        CodeIncorrect,
        CodeExpired,
        CodeAlreadyUsed,
        CodeTooManyAttempts
    }
}
