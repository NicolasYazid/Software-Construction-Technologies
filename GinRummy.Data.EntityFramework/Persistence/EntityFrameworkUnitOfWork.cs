using System.Data.Entity;

using GinRummy.Data.EntityFramework.Daos;
using GinRummy.Domain.Daos;

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
            Players = new PlayerDao(_context);
            VerificationCodes = new VerificationCodeDao(_context);
            Locales = new LocaleDao(_context);
        }

        public IPlayerDao Players { get; private set; }
        public IVerificationCodeDao VerificationCodes { get; private set; }
        public ILocaleDao Locales { get; private set; }

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
