using System;
using System.Linq;

using GinRummy.Domain.Entities.PlayerPreferences;
using GinRummy.Domain.Ports.PlayerPreferences;
using GinRummy.Persistence.Persistence;

namespace GinRummy.Persistence.Repositories
{
    /// <summary>
    /// Fulfils IPlayerPreferencesRepository using Entity Framework against GinRummy_Dev. By default,
    /// opens a short-lived context per operation; optionally reuses a context someone else
    /// already opened, so several repositories can share one transaction.
    /// </summary>
    public class PlayerPreferencesRepository : IPlayerPreferencesRepository
    {
        private readonly string _connectionStringName;
        private readonly GinRummyContext _sharedContext;

        /// <summary>
        /// Builds the repository so that each operation opens and closes its own context.
        /// </summary>
        /// <param name="connectionStringName">Name of the entry in App.config.</param>
        /// <exception cref="ArgumentNullException">Thrown when the name is null.</exception>
        public PlayerPreferencesRepository(string connectionStringName)
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
        public PlayerPreferencesRepository(GinRummyContext sharedContext)
        {
            _sharedContext = sharedContext ?? throw new ArgumentNullException(nameof(sharedContext));
        }

        /// <summary>
        /// Finds the preferences of a player by the identifier of its account, or null when they do not exist.
        /// </summary>
        /// <param name="playerAccountId">Identifier of the account that owns the preferences.</param>
        /// <returns>The matching preferences, or null.</returns>
        public PlayerPreferences FindById(int playerAccountId)
        {
            PlayerPreferences foundPlayerPreferences;
            if (_sharedContext != null)
            {
                foundPlayerPreferences = _sharedContext.PlayerPreferences.FirstOrDefault(playerPreferences => playerPreferences.PlayerAccountId == playerAccountId);
            }
            else
            {
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    foundPlayerPreferences = context.PlayerPreferences.FirstOrDefault(playerPreferences => playerPreferences.PlayerAccountId == playerAccountId);
                }
            }

            return foundPlayerPreferences;
        }

        /// <summary>
        /// Adds new preferences and persists them immediately.
        /// </summary>
        /// <param name="newPlayerPreferences">Preferences to create.</param>
        public void Add(PlayerPreferences newPlayerPreferences)
        {
            if (_sharedContext != null)
            {
                _sharedContext.PlayerPreferences.Add(newPlayerPreferences);
                _sharedContext.SaveChanges();
            }
            else
            {
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    context.PlayerPreferences.Add(newPlayerPreferences);
                    context.SaveChanges();
                }
            }
        }
    }
}