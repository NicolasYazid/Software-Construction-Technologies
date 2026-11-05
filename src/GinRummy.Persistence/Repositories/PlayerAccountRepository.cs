using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;

using GinRummy.Domain;
using GinRummy.Domain.Entities.Players;
using GinRummy.Domain.Ports.Repositories;
using GinRummy.Persistence.Persistence;

namespace GinRummy.Persistence.Repositories
{
    /// <summary>
    /// Fulfils IPlayerAccountRepository using Entity Framework against GinRummy_Dev. By
    /// default, opens a short-lived context per operation; optionally reuses a context
    /// someone else already opened, so several repositories can share one transaction.
    /// </summary>
    public class PlayerAccountRepository : IPlayerAccountRepository
    {
        private const int UniqueIndexViolationNumber = 2601;
        private const int UniqueConstraintViolationNumber = 2627;
        private readonly string _connectionStringName;
        private readonly GinRummyContext _sharedContext;

        /// <summary>
        /// Builds the repository so that each operation opens and closes its own context.
        /// </summary>
        /// <param name="connectionStringName">Name of the entry in App.config.</param>
        /// <exception cref="ArgumentNullException">Thrown when the name is null.</exception>
        public PlayerAccountRepository(string connectionStringName)
        {
            _connectionStringName = connectionStringName
                ?? throw new ArgumentNullException(nameof(connectionStringName));
        }

        /// <summary>
        /// Builds the repository so that every operation runs on a context someone else
        /// already opened, so it can share a transaction with other repositories. The
        /// caller stays responsible for disposing that context.
        /// </summary>
        /// <param name="sharedContext">Context to reuse instead of opening a new one.</param>
        /// <exception cref="ArgumentNullException">Thrown when the context is null.</exception>
        public PlayerAccountRepository(GinRummyContext sharedContext)
        {
            _sharedContext = sharedContext ?? throw new ArgumentNullException(nameof(sharedContext));
        }

        /// <summary>
        /// Finds the account whose email matches, with its player loaded, or null when no
        /// account has it.
        /// </summary>
        /// <param name="email">Email address to search for.</param>
        /// <returns>The matching account, or null.</returns>
        public PlayerAccount FindByEmail(string email)
        {
            PlayerAccount foundAccount;
            if (_sharedContext != null)
            {
                foundAccount = _sharedContext.PlayerAccounts
                    .Include(account => account.Player)
                    .FirstOrDefault(account => account.Email == email);
            }
            else
            {
                // The player is loaded in the same query because the context closes before the caller reads it.
                // A closed context can no longer load it later.
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    foundAccount = context.PlayerAccounts
                        .Include(account => account.Player)
                        .FirstOrDefault(account => account.Email == email);
                }
            }

            return foundAccount;
        }

        /// <summary>
        /// Finds the account of a player, with its player loaded, or null when the player has
        /// no account.
        /// </summary>
        /// <param name="playerId">Identifier of the player that owns the account.</param>
        /// <returns>The matching account, or null.</returns>
        public PlayerAccount FindByPlayerId(int playerId)
        {
            PlayerAccount foundAccount;
            if (_sharedContext != null)
            {
                foundAccount = _sharedContext.PlayerAccounts
                    .Include(account => account.Player)
                    .FirstOrDefault(account => account.PlayerId == playerId);
            }
            else
            {
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    foundAccount = context.PlayerAccounts
                        .Include(account => account.Player)
                        .FirstOrDefault(account => account.PlayerId == playerId);
                }
            }

            return foundAccount;
        }

        /// <summary>
        /// Adds a new account and persists it immediately.
        /// </summary>
        /// <param name="newAccount">Account to create.</param>
        /// <exception cref="EmailAlreadyRegisteredException">Thrown when another account already has the email.</exception>
        public void Add(PlayerAccount newAccount)
        {
            try
            {
                if (_sharedContext != null)
                {
                    _sharedContext.PlayerAccounts.Add(newAccount);
                    _sharedContext.SaveChanges();
                }
                else
                {
                    using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                    {
                        context.PlayerAccounts.Add(newAccount);
                        context.SaveChanges();
                    }
                }
            }
            catch (DbUpdateException ex) when (IsDuplicateKey(ex))
            {
                // Two sign-ups with the same email can both pass the earlier search.
                // The unique email of the table then rejects the second one, which is the same broken rule (CU-01 EX-01).
                throw new EmailAlreadyRegisteredException("The email already belongs to another account.", ex);
            }
        }

        /// <summary>
        /// Adds the profile of a new account and persists it immediately.
        /// </summary>
        /// <param name="newProfile">Profile to create.</param>
        public void AddProfile(PlayerProfile newProfile)
        {
            if (_sharedContext != null)
            {
                _sharedContext.PlayerProfiles.Add(newProfile);
                _sharedContext.SaveChanges();
            }
            else
            {
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    context.PlayerProfiles.Add(newProfile);
                    context.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Adds the statistics of a new account and persists them immediately.
        /// </summary>
        /// <param name="newStats">Statistics to create.</param>
        public void AddStats(PlayerStats newStats)
        {
            if (_sharedContext != null)
            {
                _sharedContext.PlayerStats.Add(newStats);
                _sharedContext.SaveChanges();
            }
            else
            {
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    context.PlayerStats.Add(newStats);
                    context.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Finds the profile of a player, with its social links and their platforms loaded, or null when the
        /// player has no profile.
        /// </summary>
        /// <param name="playerId">Identifier of the player that owns the profile.</param>
        /// <returns>The matching profile, or null.</returns>
        public PlayerProfile FindProfile(int playerId)
        {
            PlayerProfile foundProfile;
            if (_sharedContext != null)
            {
                foundProfile = FindProfileIn(_sharedContext, playerId);
            }
            else
            {
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    foundProfile = FindProfileIn(context, playerId);
                }
            }

            return foundProfile;
        }

        /// <summary>
        /// Finds the statistics of a player, with its player loaded, or null when the player has none.
        /// </summary>
        /// <param name="playerId">Identifier of the player that owns the statistics.</param>
        /// <returns>The matching statistics, or null.</returns>
        public PlayerStats FindStats(int playerId)
        {
            PlayerStats foundStats;
            if (_sharedContext != null)
            {
                foundStats = FindStatsIn(_sharedContext, playerId);
            }
            else
            {
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    foundStats = FindStatsIn(context, playerId);
                }
            }

            return foundStats;
        }

        // The links and their platforms are read in the same query, because the context closes before the profile is mapped.
        private static PlayerProfile FindProfileIn(GinRummyContext context, int playerId)
        {
            PlayerProfile foundProfile = context.PlayerProfiles
                .Include(profile => profile.SocialMediaLinks.Select(link => link.Platform))
                .FirstOrDefault(profile => profile.PlayerId == playerId);

            return foundProfile;
        }

        private static PlayerStats FindStatsIn(GinRummyContext context, int playerId)
        {
            PlayerStats foundStats = context.PlayerStats
                .Include(stats => stats.Player)
                .FirstOrDefault(stats => stats.PlayerId == playerId);

            return foundStats;
        }

        private static bool IsDuplicateKey(DbUpdateException exception)
        {
            SqlException sqlException = exception.GetBaseException() as SqlException;
            bool isDuplicateKey = false;
            if (sqlException != null)
            {
                isDuplicateKey = (sqlException.Number == UniqueIndexViolationNumber)
                    || (sqlException.Number == UniqueConstraintViolationNumber);
            }

            return isDuplicateKey;
        }
    }
}
