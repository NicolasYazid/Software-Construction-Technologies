using System;

namespace GinRummy.Domain.Daos
{
    // Only the DAOs that a use case already needs are exposed; new ones are added when a use case requires them.
    public interface IUnitOfWork : IDisposable
    {
        IPlayerDao Players { get; }
        IVerificationCodeDao VerificationCodes { get; }
        ILocaleDao Locales { get; }

        void Commit();

        void Rollback();
    }
}
