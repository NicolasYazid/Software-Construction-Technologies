using System;

namespace GinRummy.Domain
{
    /// <summary>
    /// Raised when a client presents an access token that is malformed, was altered, or has expired, so the
    /// request cannot be tied to a player.
    /// </summary>
    public class InvalidAccessTokenException : GameCoreException
    {
        /// <summary>
        /// Creates the exception without a message.
        /// </summary>
        public InvalidAccessTokenException()
        {
        }

        /// <summary>
        /// Creates the exception with a message that describes the broken rule.
        /// </summary>
        /// <param name="message">Description of the broken rule, in English.</param>
        public InvalidAccessTokenException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates the exception with a message and the exception that caused it.
        /// </summary>
        /// <param name="message">Description of the broken rule, in English.</param>
        /// <param name="innerException">Exception that caused this one.</param>
        public InvalidAccessTokenException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
