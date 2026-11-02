using System;

using GinRummy.Client.LogInServiceReference;
using GinRummy.Client.Models;

namespace GinRummy.Client.ViewModels
{
    // The sign-in screen and the two-step screen receive the same faults, so both choose their message here.
    public static class LogInFaultCommon
    {
        private const string RequiredFieldKey = "Error_ValRequiredField";
        private const string DisallowedCharactersKey = "Error_ValDisallowedCharacters";
        private const string InvalidCredentialsKey = "Error_AuthInvalidCredentials";
        private const string EmailNotVerifiedKey = "Error_AuthEmailNotVerified";
        private const string CodeNotSentKey = "Shared_CodeNotSent";
        private const string CodeInvalidFormatKey = "Error_ValInvalidCodeFormat";
        private const string CodeIncorrectKey = "Error_AuthCodeInvalid";
        private const string CodeExpiredKey = "Error_AuthCodeExpired";
        private const string CodeAlreadyUsedKey = "Error_AuthCodeAlreadyUsed";
        private const string CodeTooManyAttemptsKey = "Error_AuthTooManyAttempts";
        private const string ServiceUnavailableKey = "Error_SysServiceUnavailable";

        public static string ResolveMessageKey(ServiceFaultCode code)
        {
            string messageKey;
            switch (code)
            {
                case ServiceFaultCode.RequiredField:
                    messageKey = RequiredFieldKey;
                    break;
                case ServiceFaultCode.DisallowedCharacters:
                    messageKey = DisallowedCharactersKey;
                    break;
                case ServiceFaultCode.InvalidCredentials:
                    messageKey = InvalidCredentialsKey;
                    break;
                case ServiceFaultCode.EmailNotVerified:
                    messageKey = EmailNotVerifiedKey;
                    break;
                default:
                    messageKey = ResolveCodeMessageKey(code);
                    break;
            }

            return messageKey;
        }

        // The remaining time is measured on this computer because the server sends the end of the ban, in UTC.
        // A ban that ended while the answer travelled is shown with no time left instead of a negative one.
        public static AccountRestriction ToRestriction(SanctionFault sanction)
        {
            TimeSpan? remainingTime = null;
            if (sanction.ExpiresAt.HasValue)
            {
                TimeSpan timeLeft = sanction.ExpiresAt.Value - DateTime.UtcNow;
                remainingTime = (timeLeft > TimeSpan.Zero) ? timeLeft : TimeSpan.Zero;
            }

            return new AccountRestriction(sanction.ReasonName, remainingTime);
        }

        private static string ResolveCodeMessageKey(ServiceFaultCode code)
        {
            string messageKey;
            switch (code)
            {
                case ServiceFaultCode.CodeNotSent:
                    messageKey = CodeNotSentKey;
                    break;
                case ServiceFaultCode.CodeInvalidFormat:
                    messageKey = CodeInvalidFormatKey;
                    break;
                case ServiceFaultCode.CodeIncorrect:
                    messageKey = CodeIncorrectKey;
                    break;
                case ServiceFaultCode.CodeExpired:
                    messageKey = CodeExpiredKey;
                    break;
                case ServiceFaultCode.CodeAlreadyUsed:
                    messageKey = CodeAlreadyUsedKey;
                    break;
                case ServiceFaultCode.CodeTooManyAttempts:
                    messageKey = CodeTooManyAttemptsKey;
                    break;
                default:
                    messageKey = ServiceUnavailableKey;
                    break;
            }

            return messageKey;
        }
    }
}
