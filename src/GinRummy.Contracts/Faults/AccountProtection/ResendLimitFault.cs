using System.Runtime.Serialization;

namespace GinRummy.Contracts.Faults.AccountProtection
{
    /// <summary>
    /// Detail of a request for a new verification code refused because the limit of codes does not allow one
    /// yet (Error_AuthResendLimitReached). It carries the wait so the client can tell when to try again.
    /// </summary>
    [DataContract]
    public class ResendLimitFault
    {
        /// <summary>
        /// Seconds left until a new code may be asked for. It is a span instead of a moment, so a clock of the
        /// client that is not in step with the server does not change the wait.
        /// </summary>
        [DataMember]
        public int RetryAfterSeconds { get; set; }
    }
}
