using System;

namespace GinRummy.Domain
{
    /// <summary>
    /// Raised when the email does not belong to an account or the password does not match it. Both cases
    /// share this exception so that the player is never told which of the two failed (CU-02 EX-01).
    /// </summary>
    public class InvalidCredentialsException : GameCoreException
    {
        /// <summary>
        /// Creates the exception without a message.
        /// </summary>
        public InvalidCredentialsException()
        {
        }

        /// <summary>
        /// Creates the exception with a message that describes the broken rule.
        /// </summary>
        /// <param name="message">Description of the broken rule, in English.</param>
        public InvalidCredentialsException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates the exception with a message and the exception that caused it.
        /// </summary>
        /// <param name="message">Description of the broken rule, in English.</param>
        /// <param name="innerException">Exception that caused this one.</param>
        public InvalidCredentialsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
