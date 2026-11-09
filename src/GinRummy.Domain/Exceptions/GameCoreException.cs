using System;

namespace GinRummy.Domain
{
    /// <summary>
    /// Base of every exception the rules of the game raise. Catching it separates a broken
    /// rule of the game from an error of the language or of the framework.
    /// </summary>
    public class GameCoreException : Exception
    {
        /// <summary>
        /// Creates the exception without a message.
        /// </summary>
        public GameCoreException()
        {
        }

        /// <summary>
        /// Creates the exception with a message that describes the broken rule.
        /// </summary>
        /// <param name="message">Description of the broken rule, in English.</param>
        public GameCoreException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates the exception with a message and the exception that caused it.
        /// </summary>
        /// <param name="message">Description of the broken rule, in English.</param>
        /// <param name="innerException">Exception that caused this one.</param>
        public GameCoreException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
