using GinRummy.Domain.Entities;

namespace GinRummy.Domain.Repositories
{
    public interface IVerificationCodeRepository
    {
        VerificationCode FindMostRecent(int playerId, VerificationPurpose purpose);

        void Add(VerificationCode newVerificationCode);

        void RegisterFailedAttempt(int verificationCodeId);

        void MarkAsUsed(int verificationCodeId);
    }
}
