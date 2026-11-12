using System;

using GinRummy.Domain.Ports.AccountProtection;
using GinRummy.Domain.Shared;

namespace GinRummy.Domain.Entities.AccountProtection
{
    public class VerificationCodeRedeemer
    {
        private const int AttemptJustRegistered = 1;
        private readonly VerificationCodeChecker _codeChecker;
        private readonly IClock _clock;

        public VerificationCodeRedeemer(VerificationCodeChecker codeChecker, IClock clock)
        {
            _codeChecker = codeChecker ?? throw new ArgumentNullException(nameof(codeChecker));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        public VerificationCodeCheckResult Redeem(IVerificationCodeRepository codes, VerificationCode storedCode, string typedCode)
        {
            VerificationCodeCheckResult result = _codeChecker.Check(storedCode, typedCode);
            switch (result)
            {
                case VerificationCodeCheckResult.Valid:
                case VerificationCodeCheckResult.Expired:
                case VerificationCodeCheckResult.TooManyAttempts:
                    codes.MarkAsUsed(storedCode.VerificationCodeId, _clock.UtcNow);
                    break;
                case VerificationCodeCheckResult.Incorrect:
                    result = RegisterFailedAttempt(codes, storedCode);
                    break;
                default:
                    break;
            }

            return result;
        }

        private VerificationCodeCheckResult RegisterFailedAttempt(IVerificationCodeRepository codes, VerificationCode storedCode)
        {
            VerificationCodeCheckResult result = VerificationCodeCheckResult.Incorrect;
            codes.RegisterFailedAttempt(storedCode.VerificationCodeId);
            if ((storedCode.Attempts + AttemptJustRegistered) >= VerificationCodeChecker.MaxAttempts)
            {
                codes.MarkAsUsed(storedCode.VerificationCodeId, _clock.UtcNow);
                result = VerificationCodeCheckResult.TooManyAttempts;
            }

            return result;
        }
    }
}
