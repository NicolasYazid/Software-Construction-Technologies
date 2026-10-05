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

        public EntityFrameworkUnitOfWork(string connectionStringName)
        {
            _context = new GinRummyContext(connectionStringName);
            _transaction = _context.Database.BeginTransaction();
            _players = new PlayerDao(_context);
            _verificationCodes = new VerificationCodeDao(_context);
            _locales = new LocaleDao(_context);
        }

        public IPlayerDao Players
        {
            get { return _players; }
        }

        public IVerificationCodeDao VerificationCodes
        {
            get { return _verificationCodes; }
        }

        public ILocaleDao Locales
        {
            get { return _locales; }
        }

        public void Commit()
        {
            _transaction.Commit();
        }

        public void Rollback()
        {
            _transaction.Rollback();
        }

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
