using System;

namespace GinRummy.Domain
{
    public class VerificationResendLimitException : GameCoreException
    {
        public VerificationResendLimitException()
        {
        }

        public VerificationResendLimitException(string message)
            : base(message)
        {
        }

        public VerificationResendLimitException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public VerificationResendLimitException(string message, TimeSpan retryAfter)
            : base(message)
        {
            RetryAfter = retryAfter;
        }

        public TimeSpan RetryAfter { get; private set; }
    }
}
