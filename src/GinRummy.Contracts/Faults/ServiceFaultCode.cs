namespace GinRummy.Contracts.Faults
{
    public enum ServiceFaultCode
    {
        RequiredField,
        DisallowedCharacters,
        InvalidCredentials,
        EmailNotVerified,
        CodeNotSent,
        CodeInvalidFormat,
        CodeIncorrect,
        CodeExpired,
        CodeAlreadyUsed,
        CodeTooManyAttempts,
        SessionExpired
    }
}
