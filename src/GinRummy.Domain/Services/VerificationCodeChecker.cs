using System;
using System.Linq;

using GinRummy.Domain.Entities;
using GinRummy.Domain.Infrastructure;
using GinRummy.Domain.Security;

namespace GinRummy.Domain.Services
{
    /// <summary>
    /// Decides whether a code typed by a player is acceptable for a stored verification code.
    /// The same rules apply to every purpose (email verification, password reset, email change
    /// and two-factor), so they live here instead of being copied into each use case. It only
    /// decides: consuming an attempt or marking the code as used is left to the caller, which
    /// also persists the change.
    /// </summary>
    public class VerificationCodeChecker
    {
        /// <summary>
        /// Number of digits every verification code has.
        /// </summary>
        public const int CodeLength = 6;

        /// <summary>
        /// Wrong attempts allowed per code before it is invalidated.
        /// </summary>
        public const int MaxAttempts = 3;

        private readonly IVerificationCodeHasher _codeHasher;
        private readonly IClock _clock;

        /// <summary>
        /// Creates a checker.
        /// </summary>
        /// <param name="codeHasher">Hashes the typed code to compare it with the stored hash.</param>
        /// <param name="clock">Tells the current moment to compare it with the expiry.</param>
        public VerificationCodeChecker(IVerificationCodeHasher codeHasher, IClock clock)
        {
            _codeHasher = codeHasher ?? throw new ArgumentNullException(nameof(codeHasher));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        /// <summary>
        /// Checks a typed code against a stored one. The conditions are evaluated in a fixed
        /// order so each failure reports its own cause: format, already used, expired, too many
        /// attempts and finally the code itself.
        /// </summary>
        /// <param name="stored">Verification code kept for the player.</param>
        /// <param name="typedCode">Code exactly as the player typed it.</param>
        /// <returns>The outcome of the check.</returns>
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
            else
            {
                result = _codeHasher.VerifyCode(typedCode, stored.CodeHash)
                    ? VerificationCodeCheckResult.Valid
                    : VerificationCodeCheckResult.Incorrect;
            }

            return result;
        }

        // A malformed code is rejected before touching the stored one, so a typo does not
        // cost the player an attempt.
        private static bool HasValidFormat(string typedCode)
        {
            bool hasExpectedLength = (typedCode != null) && (typedCode.Length == CodeLength);

            return hasExpectedLength && typedCode.All(IsAsciiDigit);
        }

        // Char.IsDigit also accepts digits of other scripts, which the hash would never match.
        private static bool IsAsciiDigit(char character)
        {
            return (character >= '0') && (character <= '9');
        }
    }
}
