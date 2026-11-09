using System;

namespace GinRummy.Domain
{
    /// <summary>
    /// Raised when a typed text is longer than the column that stores it (Error_ValFieldLength).
    /// </summary>
    public class FieldTooLongException : GameCoreException
    {
        /// <summary>
        /// Creates the exception without a message.
        /// </summary>
        public FieldTooLongException()
        {
        }

        /// <summary>
        /// Creates the exception with a message that describes the broken rule.
        /// </summary>
        /// <param name="message">Description of the broken rule, in English.</param>
        public FieldTooLongException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates the exception with a message and the exception that caused it.
        /// </summary>
        /// <param name="message">Description of the broken rule, in English.</param>
        /// <param name="innerException">Exception that caused this one.</param>
        public FieldTooLongException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
