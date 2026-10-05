using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GinRummy.Data.EntityFramework.Daos;
using GinRummy.Domain.Daos;

namespace GinRummy.Data.EntityFramework.Persistence
{
    public class EntityFrameworkUnitOfWork : IUnitOfWork
    {
        private readonly GinRummyContext _context;
        private readonly DbContextTransaction _transaction;
        private readonly IPlayerDao _players;
        private readonly IVerificationCodeDao _verificationCodes;
        private readonly ILocaleDao _locales;
        private bool _isDisposed;

        /// <summary>
        /// Opens one context and one transaction, and binds every DAO to them.
        /// </summary>
        /// <param name="connectionStringName">Name of the entry in App.config.</param>
        public EntityFrameworkUnitOfWork(string connectionStringName)
        {
            _context = new GinRummyContext(connectionStringName);
            _transaction = _context.Database.BeginTransaction();
            _players = new PlayerDao(_context);
            _verificationCodes = new VerificationCodeDao(_context);
            _locales = new LocaleDao(_context);
        }

        /// <summary>
        /// Gets the player DAO bound to this unit of work.
        /// </summary>
        public IPlayerDao Players
        {
            get { return _players; }
        }

        /// <summary>
        /// Gets the verification code DAO bound to this unit of work.
        /// </summary>
        public IVerificationCodeDao VerificationCodes
        {
            get { return _verificationCodes; }
        }

        /// <summary>
        /// Gets the locale DAO bound to this unit of work.
        /// </summary>
        public ILocaleDao Locales
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
