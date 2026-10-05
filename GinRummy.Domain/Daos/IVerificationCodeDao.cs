using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GinRummy.Domain.Entities;

namespace GinRummy.Domain.Daos
{
    // Contract the game's logic uses to reach VerificationCode data, without knowing how or
    // where it is stored.
    public interface IVerificationCodeDao
    {
        VerificationCode FindMostRecent(int playerId, VerificationPurpose purpose);

        void Add(VerificationCode newVerificationCode);

        void RegisterFailedAttempt(int verificationCodeId);

        void MarkAsUsed(int verificationCodeId);
    }
}
