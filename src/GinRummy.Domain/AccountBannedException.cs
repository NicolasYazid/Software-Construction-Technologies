using System;

namespace GinRummy.Domain
{
    /// <summary>
    /// Raised when an account tries to sign in while a ban that restricts the whole account is in force
    /// (CU-02 EX-04 and EX-05). It carries the reason and the end of the ban, because the player is shown
    /// both, and a permanent ban leads to a different screen than a temporary one.
    /// </summary>
    public class AccountBannedException : GameCoreException
    {
        /// <summary>
        /// Creates the exception without a message.
        /// </summary>
        public AccountBannedException()
        {
        }

        /// <summary>
        /// Creates the exception with a message that describes the broken rule.
        /// </summary>
        /// <param name="message">Description of the broken rule, in English.</param>
        public AccountBannedException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates the exception with a message and the exception that caused it.
        /// </summary>
        /// <param name="message">Description of the broken rule, in English.</param>
        /// <param name="innerException">Exception that caused this one.</param>
        public AccountBannedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Creates the exception with a message, the reason of the ban and its end.
        /// </summary>
        /// <param name="message">Description of the broken rule, in English.</param>
        /// <param name="reasonName">Name of the reason of the ban, in the language of the player.</param>
        /// <param name="expiresAt">Moment the ban ends, in UTC; null when the ban is permanent.</param>
        public AccountBannedException(string message, string reasonName, DateTime? expiresAt)
            : base(message)
        {
            ReasonName = reasonName;
            ExpiresAt = expiresAt;
        }

        /// <summary>
        /// Gets the name of the reason of the ban, in the language of the player.
        /// </summary>
        public string ReasonName { get; private set; }

        /// <summary>
        /// Gets the moment the ban ends, in UTC; null when the ban is permanent.
        /// </summary>
        public DateTime? ExpiresAt { get; private set; }
    }
}
