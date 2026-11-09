using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;

using GinRummy.Domain;
using GinRummy.Domain.Entities.Authentication;
using GinRummy.Domain.Entities.PlayerCareer;
using GinRummy.Domain.Entities.PlayerCustomization;
using GinRummy.Domain.Ports.Authentication;
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
        /// Finds the account whose email matches, with its preferences, the language of the preferences
        /// and its profile loaded, or null when no account has it.
        /// </summary>
        /// <param name="email">Email address to search for.</param>
        /// <returns>The matching account, or null.</returns>
        public PlayerAccount FindByEmail(string email)
        {
            PlayerAccount foundAccount;
            if (_sharedContext != null)
            {
                foundAccount = _sharedContext.PlayerAccounts
                    .Include(account => account.PlayerPreferences.Locale)
                    .Include(account => account.PlayerProfile)
                    .FirstOrDefault(account => account.Email == email);
            }
            else
            {
                // The preferences and the profile are loaded in the same query because the context closes before the caller reads them.
                // A closed context can no longer load them later.
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    foundAccount = context.PlayerAccounts
                        .Include(account => account.PlayerPreferences.Locale)
                    .Include(account => account.PlayerProfile)
                        .FirstOrDefault(account => account.Email == email);
                }
            }

            return foundAccount;
        }

        /// <summary>
        /// Finds the account of a player, with its preferences and its profile loaded, or null when the player has
        /// no account.
        /// </summary>
        /// <param name="playerAccountId">Identifier of the player that owns the account.</param>
        /// <returns>The matching account, or null.</returns>
        public PlayerAccount FindByPlayerAccountId(int playerAccountId)
        {
            PlayerAccount foundAccount;
            if (_sharedContext != null)
            {
                foundAccount = _sharedContext.PlayerAccounts
                    .Include(account => account.PlayerPreferences)
                    .Include(account => account.PlayerProfile)
                    .FirstOrDefault(account => account.PlayerAccountId == playerAccountId);
            }
            else
            {
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    foundAccount = context.PlayerAccounts
                        .Include(account => account.PlayerPreferences)
                    .Include(account => account.PlayerProfile)
                        .FirstOrDefault(account => account.PlayerAccountId == playerAccountId);
                }
            }

            return foundAccount;
        }

        public PlayerProfile FindProfileByPlayerAccountId(int playerAccountId)
        {
            PlayerProfile foundProfile;
            if (_sharedContext != null)
            {
                foundProfile = _sharedContext.PlayerProfiles
                    .FirstOrDefault(playerProfile => playerProfile.PlayerAccountId == playerAccountId);
            }
            else
            {
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    foundProfile = context.PlayerProfiles
                            .FirstOrDefault(playerProfile => playerProfile.PlayerAccountId == playerAccountId);
                }
            }

            return foundProfile;
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
        /// Finds the profile of a player, with its social links loaded, or null when the player has no
        /// profile.
        /// </summary>
        /// <param name="playerAccountId">Identifier of the player that owns the profile.</param>
        /// <returns>The matching profile, or null.</returns>
        public PlayerProfile FindProfile(int playerAccountId)
        {
            PlayerProfile foundProfile;
            if (_sharedContext != null)
            {
                foundProfile = FindProfileIn(_sharedContext, playerAccountId);
            }
            else
            {
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    foundProfile = FindProfileIn(context, playerAccountId);
                }
            }

            return foundProfile;
        }

        /// <summary>
        /// Finds a player by the public tag other players see, or null when no player has it.
        /// </summary>
        /// <param name="publicTag">Public tag to search for.</param>
        /// <returns>The matching profile, or null.</returns>
        public PlayerProfile FindByPublicTag(int publicTag)
        {
            PlayerProfile foundProfile;
            if (_sharedContext != null)
            {
                foundProfile = _sharedContext.PlayerProfiles.FirstOrDefault(playerProfile => playerProfile.PublicTag == publicTag);
            }
            else
            {
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    foundProfile = context.PlayerProfiles.FirstOrDefault(playerProfile => playerProfile.PublicTag == publicTag);
                }
            }

            return foundProfile;
        }

        /// <summary>
        /// Gets every registered player.
        /// </summary>
        /// <returns>All the profiles; empty when none has registered.</returns>
        public IList<PlayerProfile> GetAllPlayers()
        {
            IList<PlayerProfile> allPlayers;
            if (_sharedContext != null)
            {
                allPlayers = _sharedContext.PlayerProfiles.ToList();
            }
            else
            {
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    allPlayers = context.PlayerProfiles.ToList();
                }
            }

            return allPlayers;
        }

        /// <summary>
        /// Finds the statistics of a player, with its profile loaded, or null when the player has none.
        /// </summary>
        /// <param name="playerAccountId">Identifier of the player that owns the statistics.</param>
        /// <returns>The matching statistics, or null.</returns>
        public PlayerStats FindStats(int playerAccountId)
        {
            PlayerStats foundStats;
            if (_sharedContext != null)
            {
                foundStats = FindStatsIn(_sharedContext, playerAccountId);
            }
            else
            {
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    foundStats = FindStatsIn(context, playerAccountId);
                }
            }

            return foundStats;
        }

        /// <summary>
        /// Adds a new social media link and persists it immediately, so its identifier is known right
        /// after the call.
        /// </summary>
        /// <param name="newLink">Link to create.</param>
        public void AddSocialMedia(SocialMedia newLink)
        {
            if (_sharedContext != null)
            {
                _sharedContext.SocialMedias.Add(newLink);
                _sharedContext.SaveChanges();
            }
            else
            {
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    context.SocialMedias.Add(newLink);
                    context.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Deletes a social media link and persists the change immediately.
        /// </summary>
        /// <param name="link">Link to delete, read through the same repository.</param>
        public void RemoveSocialMedia(SocialMedia link)
        {
            if (_sharedContext != null)
            {
                _sharedContext.SocialMedias.Remove(link);
                _sharedContext.SaveChanges();
            }
            else
            {
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    context.SocialMedias.Attach(link);
                    context.SocialMedias.Remove(link);
                    context.SaveChanges();
                }
            }
        }

        // The links are read in the same query, because the context closes before the profile is mapped.
        private static PlayerProfile FindProfileIn(GinRummyContext context, int playerAccountId)
        {
            PlayerProfile foundProfile = context.PlayerProfiles
                .Include(profile => profile.SocialMediaLinks)
                .FirstOrDefault(profile => profile.PlayerAccountId == playerAccountId);

            return foundProfile;
        }

        private static PlayerStats FindStatsIn(GinRummyContext context, int playerAccountId)
        {
            PlayerStats foundStats = context.PlayerStats
                .Include(stats => stats.PlayerProfile)
                .FirstOrDefault(stats => stats.PlayerAccountId == playerAccountId);

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
