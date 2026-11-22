namespace GinRummy.Client.ViewModels
{
    public static class MessageKeyCommon
    {
        public const string RequiredFieldKey = "Error_ValRequiredField";
        public const string DisallowedCharactersKey = "Error_ValDisallowedCharacters";
        public const string PasswordMismatchKey = "Error_ValPasswordMismatch";
        public const string ServiceUnavailableKey = "Error_SysServiceUnavailable";
        public const string ResendLimitReachedKey = "Error_AuthResendLimitReached";
        public const string CodeNotSentKey = "Shared_CodeNotSent";
        public const string CodeInvalidFormatKey = "Error_ValInvalidCodeFormat";
        public const string CodeIncorrectKey = "Error_AuthCodeInvalid";
        public const string CodeExpiredKey = "Error_AuthCodeExpired";
        public const string CodeAlreadyUsedKey = "Error_AuthCodeAlreadyUsed";
        public const string CodeTooManyAttemptsKey = "Error_AuthTooManyAttempts";
    }
}
