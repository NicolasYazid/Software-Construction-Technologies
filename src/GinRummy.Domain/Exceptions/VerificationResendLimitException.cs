using System;

namespace GinRummy.Domain
{
    /// <summary>
    /// Raised when a new verification code is asked for before the limit of codes allows it, so no code is
    /// created (CU-09 EX-08). It carries how long the player has to wait, because the message tells it.
    /// </summary>
    public class VerificationResendLimitException : GameCoreException
    {
        /// <summary>
        /// Creates the exception without a message.
        /// </summary>
        public VerificationResendLimitException()
        {
        }

        /// <summary>
        /// Creates the exception with a message that describes the broken rule.
        /// </summary>
        /// <param name="message">Description of the broken rule, in English.</param>
        public VerificationResendLimitException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates the exception with a message and the exception that caused it.
        /// </summary>
        /// <param name="message">Description of the broken rule, in English.</param>
        /// <param name="innerException">Exception that caused this one.</param>
        public VerificationResendLimitException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Creates the exception with a message and the time left until a new code may be asked for.
        /// </summary>
        /// <param name="message">Description of the broken rule, in English.</param>
        /// <param name="retryAfter">Time left until a new code may be asked for.</param>
        public VerificationResendLimitException(string message, TimeSpan retryAfter)
            : base(message)
        {
            RetryAfter = retryAfter;
        }

        /// <summary>
        /// Gets the time left until a new code may be asked for.
        /// </summary>
        public TimeSpan RetryAfter { get; private set; }
    }
}
