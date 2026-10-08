using System;

namespace GinRummy.Domain.Daos
{
    // A set of DAOs that share one transaction, so their changes are saved or discarded together.
    // The DAO list grows as new use cases need it.
    // Commit, Rollback and disposal stay the same for every use case.
    public interface IUnitOfWork : IDisposable
    {
        IPlayerDao Players { get; }
        IVerificationCodeDao VerificationCodes { get; }
        ILocaleDao Locales { get; }

        void Commit();

        void Rollback();
    }
}
