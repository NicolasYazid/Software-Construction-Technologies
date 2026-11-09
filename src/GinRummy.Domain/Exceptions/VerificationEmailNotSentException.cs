using System;

namespace GinRummy.Domain
{
    /// <summary>
    /// Raised when the mail service did not accept the email that carries a verification code. The code
    /// and its failed email are already stored, so the player can ask for a new one (CU-04 EX-03).
    /// </summary>
    public class VerificationEmailNotSentException : GameCoreException
    {
        /// <summary>
        /// Creates the exception without a message.
        /// </summary>
        public VerificationEmailNotSentException()
        {
        }

        /// <summary>
        /// Creates the exception with a message that describes the broken rule.
        /// </summary>
        /// <param name="message">Description of the broken rule, in English.</param>
        public VerificationEmailNotSentException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates the exception with a message and the exception that caused it.
        /// </summary>
        /// <param name="message">Description of the broken rule, in English.</param>
        /// <param name="innerException">Exception that caused this one.</param>
        public VerificationEmailNotSentException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
