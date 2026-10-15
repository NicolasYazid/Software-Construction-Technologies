namespace GinRummy.Domain.Services
{
    /// <summary>
    /// Outcome of checking a code a player typed against the stored verification code.
    /// </summary>
    public enum VerificationCodeCheckResult
    {
        /// <summary>
        /// The code is right and can be used.
        /// </summary>
        Valid,

        /// <summary>
        /// The text is not made of exactly six digits; no attempt is consumed.
        /// </summary>
        InvalidFormat,

        /// <summary>
        /// The stored code was already used or invalidated.
        /// </summary>
        AlreadyUsed,

        /// <summary>
        /// The stored code passed its expiry moment.
        /// </summary>
        Expired,

        /// <summary>
        /// The allowed number of wrong attempts was already spent.
        /// </summary>
        TooManyAttempts,

        /// <summary>
        /// The code does not match; one attempt must be consumed.
        /// </summary>
        Incorrect
    }
}
