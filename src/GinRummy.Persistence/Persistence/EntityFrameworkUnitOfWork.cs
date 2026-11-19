using System.Data.Entity;

using GinRummy.Domain.Ports.AccountProtection;
using GinRummy.Domain.Ports.Authentication;
using GinRummy.Domain.Ports.DirectDuel;
using GinRummy.Domain.Ports.Friendship;
using GinRummy.Domain.Ports.Gameplay;
using GinRummy.Domain.Ports.PlayerCareer;
using GinRummy.Domain.Ports.PlayerModeration;
using GinRummy.Domain.Ports.PlayerPreferences;
using GinRummy.Domain.Ports.SocialCommunication;
using GinRummy.Domain.Shared;
using GinRummy.Persistence.Repositories;

namespace GinRummy.Persistence.Persistence
{
    public class EntityFrameworkUnitOfWork : IUnitOfWork
    {
        private readonly GinRummyContext _context;
        private readonly DbContextTransaction _transaction;
        private readonly IPlayerPreferencesRepository _playerPreferences;
        private readonly IPlayerAccountRepository _playerAccounts;
        private readonly IVerificationCodeRepository _verificationCodes;
        private readonly ILocaleRepository _locales;
        private readonly IGuestSessionRepository _guestSessions;
        private readonly IFriendRepository _friends;
        private readonly IModerationRepository _moderation;
        private readonly IMessageRepository _messages;
        private readonly IChallengeRepository _challenges;
        private readonly IGameRepository _games;
        private readonly IRankingRepository _rankings;
        private bool _isDisposed;

        public EntityFrameworkUnitOfWork(string connectionStringName)
        {
            _context = new GinRummyContext(connectionStringName);
            _transaction = _context.Database.BeginTransaction();
            _playerPreferences = new PlayerPreferencesRepository(_context);
            _playerAccounts = new PlayerAccountRepository(_context);
            _verificationCodes = new VerificationCodeRepository(_context);
            _locales = new LocaleRepository(_context);
            _guestSessions = new GuestSessionRepository(_context);
            _friends = new FriendRepository(_context);
            _moderation = new ModerationRepository(_context);
            _messages = new MessageRepository(_context);
            _challenges = new ChallengeRepository(_context);
            _games = new GameRepository(_context);
            _rankings = new RankingRepository(_context);
        }

        public IPlayerPreferencesRepository PlayerPreferences
        {
            get { return _playerPreferences; }
        }

        public IPlayerAccountRepository PlayerAccounts
        {
            get { return _playerAccounts; }
        }

        public IVerificationCodeRepository VerificationCodes
        {
            get { return _verificationCodes; }
        }

        public ILocaleRepository Locales
        {
            get { return _locales; }
        }

        public IGuestSessionRepository GuestSessions
        {
            get { return _guestSessions; }
        }

        public IFriendRepository Friends
        {
            get { return _friends; }
        }

        public IModerationRepository Moderation
        {
            get { return _moderation; }
        }

        public IMessageRepository Messages
        {
            get { return _messages; }
        }

        public IChallengeRepository Challenges
        {
            get { return _challenges; }
        }

        public IGameRepository Games
        {
            get { return _games; }
        }

        public IRankingRepository Rankings
        {
            get { return _rankings; }
        }

        public void Commit()
        {
            _context.SaveChanges();
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
