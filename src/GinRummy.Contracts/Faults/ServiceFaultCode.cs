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
        CodeTooManyAttempts,

        /// <summary>
        /// The access token the client presented is malformed, altered or expired (Error_AuthSessionExpired).
        /// </summary>
        SessionExpired,

        /// <summary>
        /// The email already belongs to another account (Error_AuthEmailAlreadyRegistered).
        /// </summary>
        EmailAlreadyRegistered,

        /// <summary>
        /// The email does not have the structure of an email address (Error_ValInvalidEmailFormat).
        /// </summary>
        InvalidEmailFormat,

        /// <summary>
        /// A typed text is longer than the column that stores it (Error_ValFieldLength).
        /// </summary>
        FieldTooLong,

        /// <summary>
        /// The password does not meet the length or composition the policy asks for (Error_ValWeakPassword).
        /// </summary>
        WeakPassword,

        /// <summary>
        /// The email of the account was already verified (VerifyEmail_AlreadyVerified).
        /// </summary>
        EmailAlreadyVerified,

        /// <summary>
        /// No player has the public tag the client asked for (Error_DataNotFound).
        /// </summary>
        PlayerNotFound,

        /// <summary>
        /// The chat no longer keeps the message the player tried to report (Error_ModMessageNotAvailable).
        /// </summary>
        MessageNotFound,

        /// <summary>
        /// The picture is not a JPG or PNG image of at most 2 MB (Error_ValUnsupportedImage).
        /// </summary>
        InvalidImage,

        /// <summary>
        /// The address of a social link is not the address of a web page (Error_ValInvalidUrl).
        /// </summary>
        InvalidUrl
    }
}
