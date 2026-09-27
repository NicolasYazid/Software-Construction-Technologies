using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GinRummy.Domain.Repositories
{
    /// <summary>
    /// A set of repositories that share one transaction, so their changes are all saved
    /// together or all discarded together. The repository list grows as new use cases
    /// need it; Commit, Rollback and disposal stay the same for every use case.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Gets the player repository bound to this unit of work.
        /// </summary>
        IPlayerRepository Players { get; }

        /// <summary>
        /// Gets the verification code repository bound to this unit of work.
        /// </summary>
        IVerificationCodeRepository VerificationCodes { get; }

        /// <summary>
        /// Gets the locale repository bound to this unit of work.
        /// </summary>
        ILocaleRepository Locales { get; }

        /// <summary>
        /// Saves every change made through this unit of work as one transaction.
        /// </summary>
        void Commit();

        /// <summary>
        /// Discards every change made through this unit of work.
        /// </summary>
        void Rollback();
    }
}
