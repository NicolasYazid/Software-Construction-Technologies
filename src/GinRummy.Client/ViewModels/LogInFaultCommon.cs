using System;

using GinRummy.Client.LogInServiceReference;
using GinRummy.Client.Models;

namespace GinRummy.Client.ViewModels
{
    // The sign-in screen and the two-step screen receive the same faults, so both choose their message here.
    public static class LogInFaultCommon
    {
        private const string InvalidCredentialsKey = "Error_AuthInvalidCredentials";
        private const string EmailNotVerifiedKey = "Error_AuthEmailNotVerified";

        public static string ResolveMessageKey(ServiceFaultCode code)
        {
            string messageKey;
            switch (code)
            {
                case ServiceFaultCode.RequiredField:
                    messageKey = MessageKeyCommon.RequiredFieldKey;
                    break;
                case ServiceFaultCode.DisallowedCharacters:
                    messageKey = MessageKeyCommon.DisallowedCharactersKey;
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
                    messageKey = MessageKeyCommon.CodeNotSentKey;
                    break;
                case ServiceFaultCode.CodeInvalidFormat:
                    messageKey = MessageKeyCommon.CodeInvalidFormatKey;
                    break;
                case ServiceFaultCode.CodeIncorrect:
                    messageKey = MessageKeyCommon.CodeIncorrectKey;
                    break;
                case ServiceFaultCode.CodeExpired:
                    messageKey = MessageKeyCommon.CodeExpiredKey;
                    break;
                case ServiceFaultCode.CodeAlreadyUsed:
                    messageKey = MessageKeyCommon.CodeAlreadyUsedKey;
                    break;
                case ServiceFaultCode.CodeTooManyAttempts:
                    messageKey = MessageKeyCommon.CodeTooManyAttemptsKey;
                    break;
                default:
                    messageKey = MessageKeyCommon.ServiceUnavailableKey;
                    break;
            }

            return messageKey;
        }
    }
}
