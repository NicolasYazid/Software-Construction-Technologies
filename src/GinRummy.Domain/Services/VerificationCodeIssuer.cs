using System;

using GinRummy.Domain.Entities;
using GinRummy.Domain.Ports.Infrastructure;
using GinRummy.Domain.Ports.Repositories;
using GinRummy.Domain.Ports.Security;

namespace GinRummy.Domain.Services
{
    /// <summary>
    /// Issues verification codes: generates the value, stores only its hash and keeps a single pending code
    /// per player and purpose. Every purpose shares these rules, so they are not copied into each use case.
    /// </summary>
    public class VerificationCodeIssuer
    {
        /// <summary>
        /// Minutes a verification code stays valid after it is issued (CU-04 step 4, CU-09 FA-01).
        /// </summary>
        public const int LifetimeMinutes = 10;

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
        /// <param name="playerId">Player the code belongs to.</param>
        /// <param name="purpose">Operation the code confirms.</param>
        /// <returns>The stored code together with its plain value.</returns>
        public IssuedVerificationCode Issue(IVerificationCodeRepository codes, int playerId, VerificationPurpose purpose)
        {
            DateTime issuedAt = _clock.UtcNow;

            // A new code invalidates the one still pending for the same purpose, so only the last email works (CU-09 FA-01).
            VerificationCode previousCode = codes.FindMostRecent(playerId, purpose);
            if ((previousCode != null) && IsPending(previousCode, issuedAt))
            {
                codes.MarkAsUsed(previousCode.VerificationCodeId, issuedAt);
            }

            string plainCode = _codeGenerator.GenerateCode();
            DateTime expiresAt = issuedAt.AddMinutes(LifetimeMinutes);
            VerificationCode storedCode = new VerificationCode(
                playerId,
                purpose,
                _codeHasher.ComputeHash(plainCode),
                issuedAt,
                expiresAt);
            codes.Add(storedCode);

            return new IssuedVerificationCode(storedCode, plainCode);
        }

        private static bool IsPending(VerificationCode code, DateTime moment)
        {
            return !code.UsedAt.HasValue && (code.ExpiresAt > moment);
        }
    }
}
