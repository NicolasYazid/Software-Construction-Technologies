using System;
using System.Collections.Generic;

using GinRummy.Domain.Ports.AccountProtection;
using GinRummy.Domain.Shared;

namespace GinRummy.Domain.Entities.AccountProtection
{
    public class VerificationCodeIssuer
    {
        public const int LifetimeMinutes = 10;
        public const int ResendCooldownSeconds = 60;
        public const int MaxCodesPerWindow = 5;
        public const int LimitWindowMinutes = 60;
        private readonly IVerificationCodeGenerator _codeGenerator;
        private readonly IVerificationCodeHasher _codeHasher;
        private readonly IClock _clock;

        public VerificationCodeIssuer(IVerificationCodeGenerator codeGenerator, IVerificationCodeHasher codeHasher, IClock clock)
        {
            _codeGenerator = codeGenerator ?? throw new ArgumentNullException(nameof(codeGenerator));
            _codeHasher = codeHasher ?? throw new ArgumentNullException(nameof(codeHasher));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        public IssuedVerificationCode Issue(IVerificationCodeRepository codes, int playerAccountId, VerificationPurpose purpose)
        {
            DateTime issuedAt = _clock.UtcNow;
            VerificationCode previousCode = codes.FindMostRecent(playerAccountId, purpose);
            EnsureCooldownElapsed(previousCode, issuedAt);
            DateTime windowStart = issuedAt.AddMinutes(-LimitWindowMinutes);
            EnsureWindowHasRoom(codes.FindCreatedSince(playerAccountId, purpose, windowStart), issuedAt);

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
