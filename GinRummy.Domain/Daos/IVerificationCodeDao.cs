using GinRummy.Domain.Entities;

namespace GinRummy.Domain.Daos
{
    public interface IVerificationCodeDao
    {
        VerificationCode FindMostRecent(int playerId, VerificationPurpose purpose);

        void Add(VerificationCode newVerificationCode);

        void RegisterFailedAttempt(int verificationCodeId);

        void MarkAsUsed(int verificationCodeId);
    }
}
