using System.Collections.Generic;

using GinRummy.Client.SignUpServiceReference;

namespace GinRummy.Client.ViewModels
{
    // The sign-up screen and the verification screen receive the same faults, so both choose their message here.
    public static class SignUpFaultCommon
    {
        private const string RequiredFieldKey = "Error_ValRequiredField";
        private const string DisallowedCharactersKey = "Error_ValDisallowedCharacters";
        private const string EmailAlreadyRegisteredKey = "Error_AuthEmailAlreadyRegistered";
        private const string InvalidEmailFormatKey = "Error_ValInvalidEmailFormat";
        private const string FieldTooLongKey = "Error_ValFieldLength";
        private const string WeakPasswordKey = "Error_ValWeakPassword";
        private const string AlreadyVerifiedKey = "VerifyEmail_AlreadyVerified";
        private const string CodeNotSentKey = "Shared_CodeNotSent";
        private const string CodeInvalidFormatKey = "Error_ValInvalidCodeFormat";
        private const string CodeIncorrectKey = "Error_AuthCodeInvalid";
        private const string CodeExpiredKey = "Error_AuthCodeExpired";
        private const string CodeAlreadyUsedKey = "Error_AuthCodeAlreadyUsed";
        private const string CodeTooManyAttemptsKey = "Error_AuthTooManyAttempts";
        private const string ServiceUnavailableKey = "Error_SysServiceUnavailable";

        private static readonly IDictionary<ServiceFaultCode, string> _messageKeys = new Dictionary<ServiceFaultCode, string>
        {
            { ServiceFaultCode.RequiredField, RequiredFieldKey },
            { ServiceFaultCode.DisallowedCharacters, DisallowedCharactersKey },
            { ServiceFaultCode.EmailAlreadyRegistered, EmailAlreadyRegisteredKey },
            { ServiceFaultCode.InvalidEmailFormat, InvalidEmailFormatKey },
            { ServiceFaultCode.FieldTooLong, FieldTooLongKey },
            { ServiceFaultCode.WeakPassword, WeakPasswordKey },
            { ServiceFaultCode.EmailAlreadyVerified, AlreadyVerifiedKey },
            { ServiceFaultCode.CodeNotSent, CodeNotSentKey },
            { ServiceFaultCode.CodeInvalidFormat, CodeInvalidFormatKey },
            { ServiceFaultCode.CodeIncorrect, CodeIncorrectKey },
            { ServiceFaultCode.CodeExpired, CodeExpiredKey },
            { ServiceFaultCode.CodeAlreadyUsed, CodeAlreadyUsedKey },
            { ServiceFaultCode.CodeTooManyAttempts, CodeTooManyAttemptsKey }
        };

        public static string ResolveMessageKey(ServiceFaultCode code)
        {
            string messageKey;
            if (!_messageKeys.TryGetValue(code, out messageKey))
            {
                messageKey = ServiceUnavailableKey;
            }

            return messageKey;
        }
    }
}
