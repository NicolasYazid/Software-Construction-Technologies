using System;

namespace GinRummy.Domain
{
    /// <summary>
    /// Raised when a typed email does not have the structure of an email address or does not fit its
    /// column (CU-01 FA-02).
    /// </summary>
    public class InvalidEmailFormatException : GameCoreException
    {
        /// <summary>
        /// Creates the exception without a message.
        /// </summary>
        public InvalidEmailFormatException()
        {
        }

        /// <summary>
        /// Creates the exception with a message that describes the broken rule.
        /// </summary>
        /// <param name="message">Description of the broken rule, in English.</param>
        public InvalidEmailFormatException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates the exception with a message and the exception that caused it.
        /// </summary>
        /// <param name="message">Description of the broken rule, in English.</param>
        /// <param name="innerException">Exception that caused this one.</param>
        public InvalidEmailFormatException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
