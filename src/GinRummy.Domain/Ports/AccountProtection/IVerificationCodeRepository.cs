using System;
using System.Collections.Generic;

using GinRummy.Domain.Entities.AccountProtection;

namespace GinRummy.Domain.Ports.AccountProtection
{
    public interface IVerificationCodeRepository
    {
        VerificationCode FindMostRecent(int playerAccountId, VerificationPurpose purpose);

        IList<VerificationCode> FindCreatedSince(int playerAccountId, VerificationPurpose purpose, DateTime since);

        void Add(VerificationCode newVerificationCode);

        void AddEmail(Email newEmail);

        Email FindEmailOf(int verificationCodeId);

        void RegisterFailedAttempt(int verificationCodeId);

        void MarkAsUsed(int verificationCodeId, DateTime usedAt);
    }
}
