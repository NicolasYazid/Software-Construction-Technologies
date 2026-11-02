using System;

using GinRummy.Domain.Services;

namespace GinRummy.Domain
{
    /// <summary>
    /// Raised when a typed verification code cannot be redeemed. It carries the cause, because each one
    /// shows the player a different message (CU-09 EX-01 to EX-04 and EX-06).
    /// </summary>
    public class VerificationCodeRejectedException : GameCoreException
    {
        /// <summary>
        /// Creates the exception without a message.
        /// </summary>
        public VerificationCodeRejectedException()
        {
        }

        /// <summary>
        /// Creates the exception with a message that describes the broken rule.
        /// </summary>
        /// <param name="message">Description of the broken rule, in English.</param>
        public VerificationCodeRejectedException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates the exception with a message and the exception that caused it.
        /// </summary>
        /// <param name="message">Description of the broken rule, in English.</param>
        /// <param name="innerException">Exception that caused this one.</param>
        public VerificationCodeRejectedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Creates the exception with a message and the cause of the rejection.
        /// </summary>
        /// <param name="message">Description of the broken rule, in English.</param>
        /// <param name="reason">Outcome of the check that rejected the code.</param>
        public VerificationCodeRejectedException(string message, VerificationCodeCheckResult reason)
            : base(message)
        {
            Reason = reason;
        }

        /// <summary>
        /// Gets the outcome of the check that rejected the code.
        /// </summary>
        public VerificationCodeCheckResult Reason { get; private set; }
    }
}
