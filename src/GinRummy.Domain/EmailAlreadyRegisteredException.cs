using System;

namespace GinRummy.Domain
{
    /// <summary>
    /// Raised when the email of a new account already belongs to another account, whether it was found
    /// before saving or the database rejected it while saving (CU-01 EX-01).
    /// </summary>
    public class EmailAlreadyRegisteredException : GameCoreException
    {
        /// <summary>
        /// Creates the exception without a message.
        /// </summary>
        public EmailAlreadyRegisteredException()
        {
        }

        /// <summary>
        /// Creates the exception with a message that describes the broken rule.
        /// </summary>
        /// <param name="message">Description of the broken rule, in English.</param>
        public EmailAlreadyRegisteredException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates the exception with a message and the exception that caused it.
        /// </summary>
        /// <param name="message">Description of the broken rule, in English.</param>
        /// <param name="innerException">Exception that caused this one.</param>
        public EmailAlreadyRegisteredException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
