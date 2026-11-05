using System;

namespace GinRummy.Domain
{
    /// <summary>
    /// Raised when a new password is the same as the one the account already has (CU-06 EX-02).
    /// </summary>
    public class SamePasswordException : GameCoreException
    {
        /// <summary>
        /// Creates the exception without a message.
        /// </summary>
        public SamePasswordException()
        {
        }

        /// <summary>
        /// Creates the exception with a message that describes the broken rule.
        /// </summary>
        /// <param name="message">Description of the broken rule, in English.</param>
        public SamePasswordException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates the exception with a message and the exception that caused it.
        /// </summary>
        /// <param name="message">Description of the broken rule, in English.</param>
        /// <param name="innerException">Exception that caused this one.</param>
        public SamePasswordException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
