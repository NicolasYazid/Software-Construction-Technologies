using System;

namespace GinRummy.Domain
{
    /// <summary>
    /// Raised when an account tries to sign in before its email address was verified (CU-02 EX-03).
    /// </summary>
    public class EmailNotVerifiedException : GameCoreException
    {
        /// <summary>
        /// Creates the exception without a message.
        /// </summary>
        public EmailNotVerifiedException()
        {
        }

        /// <summary>
        /// Creates the exception with a message that describes the broken rule.
        /// </summary>
        /// <param name="message">Description of the broken rule, in English.</param>
        public EmailNotVerifiedException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates the exception with a message and the exception that caused it.
        /// </summary>
        /// <param name="message">Description of the broken rule, in English.</param>
        /// <param name="innerException">Exception that caused this one.</param>
        public EmailNotVerifiedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
