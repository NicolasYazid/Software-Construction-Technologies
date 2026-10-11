using System;

namespace GinRummy.Domain.Repositories
{
    // Only the repositories that a use case already needs are exposed; new ones are added when a use case requires them.
    public interface IUnitOfWork : IDisposable
    {
        IPlayerRepository Players { get; }
        IVerificationCodeRepository VerificationCodes { get; }
        ILocaleRepository Locales { get; }

        void Commit();

        void Rollback();
    }
}
