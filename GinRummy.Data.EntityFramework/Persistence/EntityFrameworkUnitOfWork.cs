using System.Data.Entity;

using GinRummy.Data.EntityFramework.Repositories;
using GinRummy.Domain.Repositories;

namespace GinRummy.Data.EntityFramework.Persistence
{
    public class EntityFrameworkUnitOfWork : IUnitOfWork
    {
        private readonly GinRummyContext _context;
        private readonly DbContextTransaction _transaction;
        private bool _isDisposed;

        public EntityFrameworkUnitOfWork(string connectionStringName)
        {
            _context = new GinRummyContext(connectionStringName);
            _transaction = _context.Database.BeginTransaction();
            Players = new PlayerRepository(_context);
            VerificationCodes = new VerificationCodeRepository(_context);
            Locales = new LocaleRepository(_context);
        }

        public IPlayerRepository Players { get; private set; }
        public IVerificationCodeRepository VerificationCodes { get; private set; }
        public ILocaleRepository Locales { get; private set; }

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
