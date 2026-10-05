using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GinRummy.Domain.Daos
{
    /// <summary>
    /// A set of DAOs that share one transaction, so their changes are all saved
    /// together or all discarded together. The DAO list grows as new use cases
    /// need it; Commit, Rollback and disposal stay the same for every use case.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Gets the player DAO bound to this unit of work.
        /// </summary>
        IPlayerDao Players { get; }

        /// <summary>
        /// Gets the verification code DAO bound to this unit of work.
        /// </summary>
        IVerificationCodeDao VerificationCodes { get; }

        /// <summary>
        /// Gets the locale DAO bound to this unit of work.
        /// </summary>
        ILocaleDao Locales { get; }

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
