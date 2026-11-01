using System;

namespace GinRummy.Domain
{
    /// <summary>
    /// Raised when a typed field contains characters that the game does not accept, such as emojis or
    /// line breaks (CU-02 EX-07).
    /// </summary>
    public class DisallowedCharactersException : GameCoreException
    {
        /// <summary>
        /// Creates the exception without a message.
        /// </summary>
        public DisallowedCharactersException()
        {
        }

        /// <summary>
        /// Creates the exception with a message that describes the broken rule.
        /// </summary>
        /// <param name="message">Description of the broken rule, in English.</param>
        public DisallowedCharactersException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates the exception with a message and the exception that caused it.
        /// </summary>
        /// <param name="message">Description of the broken rule, in English.</param>
        /// <param name="innerException">Exception that caused this one.</param>
        public DisallowedCharactersException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
