using System;
using System.ServiceModel;

using GinRummy.Contracts.Faults;
using GinRummy.Domain;
using GinRummy.Domain.Entities.AccountProtection;

namespace GinRummy.Services.Services
{
    /// <summary>
    /// Builds the faults that more than one service declares, so the sign-in and the sign-up report the same
    /// broken rule with the same fault.
    /// </summary>
    public static class ServiceFaultCommon
    {
        /// <summary>
        /// Builds the fault of a controlled failure.
        /// </summary>
        /// <param name="code">Cause of the failure.</param>
        /// <returns>The fault, ready to be thrown.</returns>
        public static FaultException<ServiceFault> CreateServiceFault(ServiceFaultCode code)
        {
            return new FaultException<ServiceFault>(new ServiceFault { Code = code });
        }

        /// <summary>
        /// Builds the fault of a typed verification code that was rejected.
        /// </summary>
        /// <param name="reason">Outcome of the check that rejected the code.</param>
        /// <returns>The fault, ready to be thrown.</returns>
        public static FaultException<ServiceFault> CreateRejectedCodeFault(VerificationCodeCheckResult reason)
        {
            return CreateServiceFault(MapRejectionReason(reason));
        }

        /// <summary>
        /// Builds the fault of a new code asked for before the limit of codes allows it.
        /// </summary>
        /// <param name="exception">Exception that carries the time left until a new code may be asked for.</param>
        /// <returns>The fault, ready to be thrown.</returns>
        public static FaultException<ResendLimitFault> CreateResendLimitFault(VerificationResendLimitException exception)
        {
            // The wait is rounded up, so a client that waits the seconds it was told is never refused again.
            ResendLimitFault resendLimit = new ResendLimitFault
            {
                RetryAfterSeconds = (int)Math.Ceiling(exception.RetryAfter.TotalSeconds)
            };

            return new FaultException<ResendLimitFault>(resendLimit);
        }

        private static ServiceFaultCode MapRejectionReason(VerificationCodeCheckResult reason)
        {
            ServiceFaultCode code;
            switch (reason)
            {
                case VerificationCodeCheckResult.InvalidFormat:
                    code = ServiceFaultCode.CodeInvalidFormat;
                    break;
                case VerificationCodeCheckResult.AlreadyUsed:
                    code = ServiceFaultCode.CodeAlreadyUsed;
                    break;
                case VerificationCodeCheckResult.Expired:
                    code = ServiceFaultCode.CodeExpired;
                    break;
                case VerificationCodeCheckResult.TooManyAttempts:
                    code = ServiceFaultCode.CodeTooManyAttempts;
                    break;
                default:
                    code = ServiceFaultCode.CodeIncorrect;
                    break;
            }

            return code;
        }
    }
}
