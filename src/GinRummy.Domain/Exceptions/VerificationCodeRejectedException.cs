using System;

using GinRummy.Domain.Entities.AccountProtection;

namespace GinRummy.Domain
{
    public class VerificationCodeRejectedException : GameCoreException
    {
        public VerificationCodeRejectedException()
        {
        }

        public VerificationCodeRejectedException(string message)
            : base(message)
        {
        }

        public VerificationCodeRejectedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public VerificationCodeRejectedException(string message, VerificationCodeCheckResult reason)
            : base(message)
        {
            Reason = reason;
        }

        public VerificationCodeCheckResult Reason { get; private set; }
    }
}
