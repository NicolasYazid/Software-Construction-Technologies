using System;

namespace GinRummy.Domain
{
    /// <summary>
    /// Raised when a new password does not meet the length or composition the policy asks for (CU-01 FA-03).
    /// </summary>
    public class WeakPasswordException : GameCoreException
    {
        /// <summary>
        /// Creates the exception without a message.
        /// </summary>
        public WeakPasswordException()
        {
        }

        /// <summary>
        /// Creates the exception with a message that describes the broken rule.
        /// </summary>
        /// <param name="message">Description of the broken rule, in English.</param>
        public WeakPasswordException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates the exception with a message and the exception that caused it.
        /// </summary>
        /// <param name="message">Description of the broken rule, in English.</param>
        /// <param name="innerException">Exception that caused this one.</param>
        public WeakPasswordException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
