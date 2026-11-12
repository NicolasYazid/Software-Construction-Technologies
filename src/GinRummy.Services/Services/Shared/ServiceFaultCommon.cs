using System;
using System.ServiceModel;

using GinRummy.Contracts.Faults.AccountProtection;
using GinRummy.Contracts.Faults.Shared;
using GinRummy.Domain;
using GinRummy.Domain.Entities.AccountProtection;

namespace GinRummy.Services.Services.Shared
{
    public static class ServiceFaultCommon
    {
        public static FaultException<ServiceFault> CreateServiceFault(ServiceFaultCode code)
        {
            return new FaultException<ServiceFault>(new ServiceFault { Code = code });
        }

        public static FaultException<ServiceFault> CreateRejectedCodeFault(VerificationCodeCheckResult reason)
        {
            return CreateServiceFault(MapRejectionReason(reason));
        }

        public static FaultException<ResendLimitFault> CreateResendLimitFault(VerificationResendLimitException exception)
        {
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
