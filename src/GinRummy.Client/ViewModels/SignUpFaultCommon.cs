using System.Collections.Generic;

using GinRummy.Client.SignUpServiceReference;

namespace GinRummy.Client.ViewModels
{
    // The sign-up screen and the verification screen receive the same faults, so both choose their message here.
    public static class SignUpFaultCommon
    {
        private const string EmailAlreadyRegisteredKey = "Error_AuthEmailAlreadyRegistered";
        private const string InvalidEmailFormatKey = "Error_ValInvalidEmailFormat";
        private const string FieldTooLongKey = "Error_ValFieldLength";
        private const string WeakPasswordKey = "Error_ValWeakPassword";
        private const string AlreadyVerifiedKey = "VerifyEmail_AlreadyVerified";

        private static readonly IDictionary<ServiceFaultCode, string> s_messageKeys = new Dictionary<ServiceFaultCode, string>
        {
            { ServiceFaultCode.RequiredField, MessageKeyCommon.RequiredFieldKey },
            { ServiceFaultCode.DisallowedCharacters, MessageKeyCommon.DisallowedCharactersKey },
            { ServiceFaultCode.EmailAlreadyRegistered, EmailAlreadyRegisteredKey },
            { ServiceFaultCode.InvalidEmailFormat, InvalidEmailFormatKey },
            { ServiceFaultCode.FieldTooLong, FieldTooLongKey },
            { ServiceFaultCode.WeakPassword, WeakPasswordKey },
            { ServiceFaultCode.EmailAlreadyVerified, AlreadyVerifiedKey },
            { ServiceFaultCode.CodeNotSent, MessageKeyCommon.CodeNotSentKey },
            { ServiceFaultCode.CodeInvalidFormat, MessageKeyCommon.CodeInvalidFormatKey },
            { ServiceFaultCode.CodeIncorrect, MessageKeyCommon.CodeIncorrectKey },
            { ServiceFaultCode.CodeExpired, MessageKeyCommon.CodeExpiredKey },
            { ServiceFaultCode.CodeAlreadyUsed, MessageKeyCommon.CodeAlreadyUsedKey },
            { ServiceFaultCode.CodeTooManyAttempts, MessageKeyCommon.CodeTooManyAttemptsKey }
        };

        public static string ResolveMessageKey(ServiceFaultCode code)
        {
            string messageKey;
            if (!s_messageKeys.TryGetValue(code, out messageKey))
            {
                messageKey = MessageKeyCommon.ServiceUnavailableKey;
            }

            return messageKey;
        }
    }
}
