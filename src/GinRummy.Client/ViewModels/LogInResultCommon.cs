using System;

using GinRummy.Client.LogInServiceReference;
using GinRummy.Client.Models;

namespace GinRummy.Client.ViewModels
{
    // The sign-in screen and the two-step screen receive the same answers, so both choose their message here.
    public static class LogInResultCommon
    {
        private const string InvalidCredentialsKey = "Error_AuthInvalidCredentials";
        private const string EmailNotVerifiedKey = "Error_AuthEmailNotVerified";
        private const string RequiredFieldKey = "Error_ValRequiredField";
        private const string DisallowedCharactersKey = "Error_ValDisallowedCharacters";
        private const string ResendLimitReachedKey = "Error_AuthResendLimitReached";
        private const string ServiceUnavailableKey = "Error_SysServiceUnavailable";
        private const string CodeNotSentKey = "Shared_CodeNotSent";
        private const string CodeInvalidFormatKey = "Error_ValInvalidCodeFormat";
        private const string CodeIncorrectKey = "Error_AuthCodeInvalid";
        private const string CodeExpiredKey = "Error_AuthCodeExpired";
        private const string CodeAlreadyUsedKey = "Error_AuthCodeAlreadyUsed";
        private const string CodeTooManyAttemptsKey = "Error_AuthTooManyAttempts";

        public static string ResolveMessageKey(LogInStatus status)
        {
            string messageKey;
            switch (status)
            {
                case LogInStatus.RequiredField:
                    messageKey = RequiredFieldKey;
                    break;
                case LogInStatus.DisallowedCharacters:
                    messageKey = DisallowedCharactersKey;
                    break;
                case LogInStatus.InvalidCredentials:
                    messageKey = InvalidCredentialsKey;
                    break;
                case LogInStatus.EmailNotVerified:
                    messageKey = EmailNotVerifiedKey;
                    break;
                case LogInStatus.ResendLimitReached:
                    messageKey = ResendLimitReachedKey;
                    break;
                default:
                    messageKey = ResolveCodeMessageKey(status);
                    break;
            }

            return messageKey;
        }

        // The mail server is the only failure of the sign-in that the server declares; any other fault is unexpected.
        public static string ResolveFaultMessageKey(ServiceFaultCode code)
        {
            return code == ServiceFaultCode.CodeNotSent ? CodeNotSentKey : ServiceUnavailableKey;
        }

        // The remaining time is measured on this computer because the server sends the end of the ban, in UTC.
        // A ban that ended while the answer travelled is shown with no time left instead of a negative one.
        public static AccountRestriction ToRestriction(LogInOutcome outcome)
        {
            TimeSpan? remainingTime = null;
            if (outcome.SanctionExpiresAt.HasValue)
            {
                TimeSpan timeLeft = outcome.SanctionExpiresAt.Value - DateTime.UtcNow;
                remainingTime = (timeLeft > TimeSpan.Zero) ? timeLeft : TimeSpan.Zero;
            }

            return new AccountRestriction(outcome.SanctionReasonName, remainingTime);
        }

        private static string ResolveCodeMessageKey(LogInStatus status)
        {
            string messageKey;
            switch (status)
            {
                case LogInStatus.CodeInvalidFormat:
                    messageKey = CodeInvalidFormatKey;
                    break;
                case LogInStatus.CodeIncorrect:
                    messageKey = CodeIncorrectKey;
                    break;
                case LogInStatus.CodeExpired:
                    messageKey = CodeExpiredKey;
                    break;
                case LogInStatus.CodeAlreadyUsed:
                    messageKey = CodeAlreadyUsedKey;
                    break;
                case LogInStatus.CodeTooManyAttempts:
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
