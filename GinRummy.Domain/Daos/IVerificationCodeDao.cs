using GinRummy.Domain.Entities;

namespace GinRummy.Domain.Daos
{
    // Contract the game's logic uses to reach VerificationCode data.
    // The logic does not need to know how or where the data is stored.
    public interface IVerificationCodeDao
    {
        VerificationCode FindMostRecent(int playerId, VerificationPurpose purpose);

        void Add(VerificationCode newVerificationCode);

        void RegisterFailedAttempt(int verificationCodeId);

        void MarkAsUsed(int verificationCodeId);
    }
}
