using System;
using System.Collections.Generic;

using GinRummy.Domain.Entities.Authentication;
using GinRummy.Domain.Ports.Infrastructure;
using GinRummy.Domain.Ports.Repositories;
using GinRummy.Domain.Ports.Security;

namespace GinRummy.Domain.Services
{
    /// <summary>
    /// Issues verification codes: generates the value, stores only its hash, keeps a single pending code
    /// per player and purpose, and limits how often a new one may be asked for. Every purpose shares these
    /// rules, so they are not copied into each use case.
    /// </summary>
    public class VerificationCodeIssuer
    {
        /// <summary>
        /// Minutes a verification code stays valid after it is issued (CU-04 step 4, CU-09 FA-01).
        /// </summary>
        public const int LifetimeMinutes = 10;
        /// <summary>
        /// Seconds that must pass after a code is issued before another one for the same purpose (CU-09 EX-08).
        /// </summary>
        public const int ResendCooldownSeconds = 60;
        /// <summary>
        /// Codes that a player may receive for the same purpose within the limit window (CU-09 EX-08).
        /// </summary>
        public const int MaxCodesPerWindow = 5;
        /// <summary>
        /// Minutes of the window in which the codes of a player and a purpose are counted.
        /// </summary>
        public const int LimitWindowMinutes = 60;
        private readonly IVerificationCodeGenerator _codeGenerator;
        private readonly IVerificationCodeHasher _codeHasher;
        private readonly IClock _clock;

        /// <summary>
        /// Builds the issuer with the generator, the hasher and the clock the composition root assembled.
        /// </summary>
        /// <param name="codeGenerator">Generates the plain value of each code.</param>
        /// <param name="codeHasher">Hashes the value before it is stored.</param>
        /// <param name="clock">Tells the moment the code is created and from which it expires.</param>
        /// <exception cref="ArgumentNullException">Thrown when a dependency is null.</exception>
        public VerificationCodeIssuer(IVerificationCodeGenerator codeGenerator, IVerificationCodeHasher codeHasher, IClock clock)
        {
            _codeGenerator = codeGenerator ?? throw new ArgumentNullException(nameof(codeGenerator));
            _codeHasher = codeHasher ?? throw new ArgumentNullException(nameof(codeHasher));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        /// <summary>
        /// Issues a new code for a player and stores it.
        /// </summary>
        /// <param name="codes">Repository of the codes, bound to the transaction of the caller.</param>
        /// <param name="playerAccountId">Player the code belongs to.</param>
        /// <param name="purpose">Operation the code confirms.</param>
        /// <returns>The stored code together with its plain value.</returns>
        /// <exception cref="VerificationResendLimitException">Thrown when the limit of codes does not allow a new one yet.</exception>
        public IssuedVerificationCode Issue(IVerificationCodeRepository codes, int playerAccountId, VerificationPurpose purpose)
        {
            DateTime issuedAt = _clock.UtcNow;
            VerificationCode previousCode = codes.FindMostRecent(playerAccountId, purpose);
            EnsureCooldownElapsed(previousCode, issuedAt);
            DateTime windowStart = issuedAt.AddMinutes(-LimitWindowMinutes);
            EnsureWindowHasRoom(codes.FindCreatedSince(playerAccountId, purpose, windowStart), issuedAt);

            // A new code invalidates the one still pending for the same purpose, so only the last email works (CU-09 FA-01).
            if ((previousCode != null) && IsPending(previousCode, issuedAt))
            {
                codes.MarkAsUsed(previousCode.VerificationCodeId, issuedAt);
            }

            string plainCode = _codeGenerator.GenerateCode();
            DateTime expiresAt = issuedAt.AddMinutes(LifetimeMinutes);
            VerificationCode storedCode = new VerificationCode(
                playerAccountId,
                purpose,
                _codeHasher.ComputeHash(plainCode),
                issuedAt,
                expiresAt);
            codes.Add(storedCode);

            return new IssuedVerificationCode(storedCode, plainCode);
        }

        // Each email costs a delivery, so a code asked for right after the previous one is refused instead of sent.
        private static void EnsureCooldownElapsed(VerificationCode previousCode, DateTime moment)
        {
            if (previousCode != null)
            {
                DateTime allowedAt = previousCode.CreatedAt.AddSeconds(ResendCooldownSeconds);
                if (allowedAt > moment)
                {
                    throw new VerificationResendLimitException("A new code was asked for too soon.", allowedAt - moment);
                }
            }
        }

        // The cooldown alone still allows one email per minute, so the window caps how many reach the same inbox in an hour.
        // The window has room again once enough of the oldest codes in it leave it.
        private static void EnsureWindowHasRoom(IList<VerificationCode> recentCodes, DateTime moment)
        {
            if (recentCodes.Count >= MaxCodesPerWindow)
            {
                VerificationCode codeThatFreesRoom = recentCodes[recentCodes.Count - MaxCodesPerWindow];
                DateTime allowedAt = codeThatFreesRoom.CreatedAt.AddMinutes(LimitWindowMinutes);
                throw new VerificationResendLimitException("The limit of codes in the window was reached.", allowedAt - moment);
            }
        }

        private static bool IsPending(VerificationCode code, DateTime moment)
        {
            return !code.UsedAt.HasValue && (code.ExpiresAt > moment);
        }
    }
}
