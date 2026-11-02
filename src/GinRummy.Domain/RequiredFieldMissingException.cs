using System;

namespace GinRummy.Domain
{
    /// <summary>
    /// Raised when a field that an operation needs arrives without any character (CU-02 FA-02).
    /// </summary>
    public class RequiredFieldMissingException : GameCoreException
    {
        /// <summary>
        /// Creates the exception without a message.
        /// </summary>
        public RequiredFieldMissingException()
        {
        }

        /// <summary>
        /// Creates the exception with a message that describes the broken rule.
        /// </summary>
        /// <param name="message">Description of the broken rule, in English.</param>
        public RequiredFieldMissingException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates the exception with a message and the exception that caused it.
        /// </summary>
        /// <param name="message">Description of the broken rule, in English.</param>
        /// <param name="innerException">Exception that caused this one.</param>
        public RequiredFieldMissingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
