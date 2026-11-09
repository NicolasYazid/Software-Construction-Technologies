using System;

namespace GinRummy.Domain
{
    /// <summary>
    /// Raised when the email of an account is verified again, or a new code is asked for it, after it
    /// was already verified (CU-09 FA-02).
    /// </summary>
    public class EmailAlreadyVerifiedException : GameCoreException
    {
        /// <summary>
        /// Creates the exception without a message.
        /// </summary>
        public EmailAlreadyVerifiedException()
        {
        }

        /// <summary>
        /// Creates the exception with a message that describes the broken rule.
        /// </summary>
        /// <param name="message">Description of the broken rule, in English.</param>
        public EmailAlreadyVerifiedException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates the exception with a message and the exception that caused it.
        /// </summary>
        /// <param name="message">Description of the broken rule, in English.</param>
        /// <param name="innerException">Exception that caused this one.</param>
        public EmailAlreadyVerifiedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
