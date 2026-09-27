using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GinRummy.Data.EntityFramework.Repositories;
using GinRummy.Domain.Repositories;

namespace GinRummy.Data.EntityFramework.Persistence
{
    public class EntityFrameworkUnitOfWork : IUnitOfWork
    {
        private readonly GinRummyContext _context;
        private readonly DbContextTransaction _transaction;
        private readonly IPlayerRepository _players;
        private readonly IVerificationCodeRepository _verificationCodes;
        private readonly ILocaleRepository _locales;
        private bool _isDisposed;

        /// <summary>
        /// Opens one context and one transaction, and binds every repository to them.
        /// </summary>
        /// <param name="connectionStringName">Name of the entry in App.config.</param>
        public EntityFrameworkUnitOfWork(string connectionStringName)
        {
            _context = new GinRummyContext(connectionStringName);
            _transaction = _context.Database.BeginTransaction();
            _players = new PlayerRepository(_context);
            _verificationCodes = new VerificationCodeRepository(_context);
            _locales = new LocaleRepository(_context);
        }

        /// <summary>
        /// Gets the player repository bound to this unit of work.
        /// </summary>
        public IPlayerRepository Players
        {
            get { return _players; }
        }

        /// <summary>
        /// Gets the verification code repository bound to this unit of work.
        /// </summary>
        public IVerificationCodeRepository VerificationCodes
        {
            get { return _verificationCodes; }
        }

        /// <summary>
        /// Gets the locale repository bound to this unit of work.
        /// </summary>
        public ILocaleRepository Locales
        {
            get { return _locales; }
        }

        /// <summary>
        /// Saves every change made through this unit of work as one transaction.
        /// </summary>
        public void Commit()
        {
            _transaction.Commit();
        }

        /// <summary>
        /// Discards every change made through this unit of work.
        /// </summary>
        public void Rollback()
        {
            _transaction.Rollback();
        }

        /// <summary>
        /// Frees the transaction and the context.
        /// </summary>
        public void Dispose()
        {
            if (!_isDisposed)
            {
                _transaction.Dispose();
                _context.Dispose();
                _isDisposed = true;
            }
        }
    }
}
