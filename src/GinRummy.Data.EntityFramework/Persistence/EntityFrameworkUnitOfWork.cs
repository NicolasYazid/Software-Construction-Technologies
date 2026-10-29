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
        private readonly IPlayerAccountRepository _playerAccounts;
        private readonly IVerificationCodeRepository _verificationCodes;
        private readonly ILocaleRepository _locales;
        private readonly IPlayerSessionRepository _playerSessions;
        private readonly IFriendRepository _friends;
        private readonly IModerationRepository _moderation;
        private readonly IMessageRepository _messages;
        private readonly IChallengeRepository _challenges;
        private readonly IGameRepository _games;
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
            _playerAccounts = new PlayerAccountRepository(_context);
            _verificationCodes = new VerificationCodeRepository(_context);
            _locales = new LocaleRepository(_context);
            _playerSessions = new PlayerSessionRepository(_context);
            _friends = new FriendRepository(_context);
            _moderation = new ModerationRepository(_context);
            _messages = new MessageRepository(_context);
            _challenges = new ChallengeRepository(_context);
            _games = new GameRepository(_context);
        }

        /// <summary>
        /// Gets the player repository bound to this unit of work.
        /// </summary>
        public IPlayerRepository Players
        {
            get { return _players; }
        }

        /// <summary>
        /// Gets the player account repository bound to this unit of work.
        /// </summary>
        public IPlayerAccountRepository PlayerAccounts
        {
            get { return _playerAccounts; }
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
        /// Gets the player session repository bound to this unit of work.
        /// </summary>
        public IPlayerSessionRepository PlayerSessions
        {
            get { return _playerSessions; }
        }

        /// <summary>
        /// Gets the friend request and friendship repository bound to this unit of work.
        /// </summary>
        public IFriendRepository Friends
        {
            get { return _friends; }
        }

        /// <summary>
        /// Gets the warning and ban repository bound to this unit of work.
        /// </summary>
        public IModerationRepository Moderation
        {
            get { return _moderation; }
        }

        /// <summary>
        /// Gets the chat message repository bound to this unit of work.
        /// </summary>
        public IMessageRepository Messages
        {
            get { return _messages; }
        }

        /// <summary>
        /// Gets the challenge repository bound to this unit of work.
        /// </summary>
        public IChallengeRepository Challenges
        {
            get { return _challenges; }
        }

        /// <summary>
        /// Gets the match and match history repository bound to this unit of work.
        /// </summary>
        public IGameRepository Games
        {
            get { return _games; }
        }

        /// <summary>
        /// Saves every change made through this unit of work as one transaction, including
        /// changes made by calling methods on entities the repositories returned.
        /// </summary>
        public void Commit()
        {
            _context.SaveChanges();
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
