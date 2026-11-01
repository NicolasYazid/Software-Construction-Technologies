using System;

using GinRummy.Domain.Entities;
using GinRummy.Domain.Ports.Infrastructure;
using GinRummy.Domain.Ports.Repositories;

namespace GinRummy.Domain.Services
{
    /// <summary>
    /// Redeems a typed verification code: asks the checker for its outcome and applies to the stored code
    /// the change that outcome requires (CU-09 steps 4 to 7). Every purpose shares these changes.
    /// </summary>
    public class VerificationCodeRedeemer
    {
        private const int AttemptJustRegistered = 1;

        private readonly VerificationCodeChecker _codeChecker;
        private readonly IClock _clock;

        /// <summary>
        /// Builds the redeemer with the checker and the clock the composition root assembled.
        /// </summary>
        /// <param name="codeChecker">Decides the outcome of a typed code.</param>
        /// <param name="clock">Tells the moment a code stops being usable.</param>
        /// <exception cref="ArgumentNullException">Thrown when a dependency is null.</exception>
        public VerificationCodeRedeemer(VerificationCodeChecker codeChecker, IClock clock)
        {
            _codeChecker = codeChecker ?? throw new ArgumentNullException(nameof(codeChecker));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        /// <summary>
        /// Checks a typed code against the stored one and records the result in the stored code.
        /// </summary>
        /// <param name="codes">Repository of the codes, bound to the transaction of the caller.</param>
        /// <param name="storedCode">Code kept for the player.</param>
        /// <param name="typedCode">Code exactly as the player typed it.</param>
        /// <returns>The outcome; TooManyAttempts when this attempt used the last one.</returns>
        public VerificationCodeCheckResult Redeem(IVerificationCodeRepository codes, VerificationCode storedCode, string typedCode)
        {
            VerificationCodeCheckResult result = _codeChecker.Check(storedCode, typedCode);
            switch (result)
            {
                case VerificationCodeCheckResult.Valid:
                case VerificationCodeCheckResult.Expired:
                case VerificationCodeCheckResult.TooManyAttempts:
                    // A redeemed, expired or exhausted code can never be used again (CU-09 step 7, EX-02 and EX-04).
                    codes.MarkAsUsed(storedCode.VerificationCodeId, _clock.UtcNow);
                    break;
                case VerificationCodeCheckResult.Incorrect:
                    result = RegisterFailedAttempt(codes, storedCode);
                    break;
                default:
                    // A malformed or already used code changes nothing, so a typo does not cost an attempt (CU-09 EX-03 and EX-06).
                    break;
            }

            return result;
        }

        // The attempt that reaches the limit also invalidates the code, so the player is asked for a new one (CU-09 EX-04).
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
