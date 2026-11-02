namespace GinRummy.Contracts.Faults
{
    /// <summary>
    /// Causes of a controlled failure that the client tells apart to choose its message.
    /// </summary>
    public enum ServiceFaultCode
    {
        /// <summary>
        /// A required field arrived empty (Error_ValRequiredField).
        /// </summary>
        RequiredField,

        /// <summary>
        /// A field contains characters the game does not accept (Error_ValDisallowedCharacters).
        /// </summary>
        DisallowedCharacters,

        /// <summary>
        /// The email or the password is not valid, without telling which (Error_AuthInvalidCredentials).
        /// </summary>
        InvalidCredentials,

        /// <summary>
        /// The email of the account was never verified (Error_AuthEmailNotVerified).
        /// </summary>
        EmailNotVerified,

        /// <summary>
        /// The email with the verification code could not be sent (Shared_CodeNotSent).
        /// </summary>
        CodeNotSent,

        /// <summary>
        /// The verification code does not have six digits (Error_ValInvalidCodeFormat).
        /// </summary>
        CodeInvalidFormat,

        /// <summary>
        /// The verification code does not match the one emailed (Error_AuthCodeInvalid).
        /// </summary>
        CodeIncorrect,

        /// <summary>
        /// The verification code expired or no longer exists (Error_AuthCodeExpired).
        /// </summary>
        CodeExpired,

        /// <summary>
        /// The verification code was already redeemed (Error_AuthCodeAlreadyUsed).
        /// </summary>
        CodeAlreadyUsed,

        /// <summary>
        /// The verification code ran out of attempts (Error_AuthTooManyAttempts).
        /// </summary>
        CodeTooManyAttempts
    }
}
