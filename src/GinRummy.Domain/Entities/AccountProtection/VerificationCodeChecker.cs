using System;
using System.Linq;

using GinRummy.Domain.Ports.AccountProtection;
using GinRummy.Domain.Shared;

namespace GinRummy.Domain.Entities.AccountProtection
{
    public class VerificationCodeChecker
    {
        public const int CodeLength = 6;

        public const int MaxAttempts = 3;

        private readonly IVerificationCodeHasher _codeHasher;
        private readonly IClock _clock;

        public VerificationCodeChecker(IVerificationCodeHasher codeHasher, IClock clock)
        {
            _codeHasher = codeHasher ?? throw new ArgumentNullException(nameof(codeHasher));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        public VerificationCodeCheckResult Check(VerificationCode stored, string typedCode)
        {
            if (stored == null)
            {
                throw new ArgumentNullException(nameof(stored));
            }

            VerificationCodeCheckResult result;

            if (!HasValidFormat(typedCode))
            {
                result = VerificationCodeCheckResult.InvalidFormat;
            }
            else if (stored.UsedAt.HasValue)
            {
                result = VerificationCodeCheckResult.AlreadyUsed;
            }
            else if (stored.ExpiresAt <= _clock.UtcNow)
            {
                result = VerificationCodeCheckResult.Expired;
            }
            else if (stored.Attempts >= MaxAttempts)
            {
                result = VerificationCodeCheckResult.TooManyAttempts;
            }
            else if (_codeHasher.VerifyCode(typedCode, stored.CodeHash))
            {
                result = VerificationCodeCheckResult.Valid;
            }
            else
            {
                result = VerificationCodeCheckResult.Incorrect;
            }

            return result;
        }

        private static bool HasValidFormat(string typedCode)
        {
            bool hasExpectedLength = (typedCode != null) && (typedCode.Length == CodeLength);

            return hasExpectedLength && typedCode.All(IsAsciiDigit);
        }

        private static bool IsAsciiDigit(char character)
        {
            return (character >= '0') && (character <= '9');
        }
    }
}
