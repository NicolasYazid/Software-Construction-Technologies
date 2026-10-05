using GinRummy.Data.EntityFramework.Persistence;
using GinRummy.Domain.Daos;
using GinRummy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GinRummy.Data.EntityFramework.Daos
{
    /// <summary>
    /// Fulfils IPlayerDao using Entity Framework against GinRummy_Dev. By default,
    /// opens a short-lived context per operation; optionally reuses a context someone else
    /// already opened, so several DAOs can share one transaction.
    /// </summary>
    public class PlayerDao : IPlayerDao
    {
        private readonly string _connectionStringName;
        private readonly GinRummyContext _sharedContext;

        /// <summary>
        /// Builds the DAO so that each operation opens and closes its own context.
        /// </summary>
        /// <param name="connectionStringName">Name of the entry in App.config.</param>
        public PlayerDao(string connectionStringName)
        {
            _connectionStringName = connectionStringName;
        }

        /// <summary>
        /// Builds the DAO so that every operation runs on a context someone else
        /// already opened, so it can share a transaction with other DAOs. The
        /// caller stays responsible for disposing that context.
        /// </summary>
        /// <param name="sharedContext">Context to reuse instead of opening a new one.</param>
        public PlayerDao(GinRummyContext sharedContext)
        {
            _sharedContext = sharedContext;
        }

        /// <summary>
        /// Finds the player whose email matches, or null when no player has it.
        /// </summary>
        /// <param name="email">Email address to search for.</param>
        /// <returns>The matching player, or null.</returns>
        public Player FindByEmail(string email)
        {
            Player foundPlayer;
            if (_sharedContext != null)
            {
                // Not wrapped in using: the caller opened this context and stays
                // responsible for closing it.
                foundPlayer = _sharedContext.Players.FirstOrDefault(player => player.Email == email);
            }
            else
            {
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    foundPlayer = context.Players.FirstOrDefault(player => player.Email == email);
                }
            }

            return foundPlayer;
        }

        /// <summary>
        /// Adds a new player and persists it immediately.
        /// </summary>
        /// <param name="newPlayer">Player to create.</param>
        public void Add(Player newPlayer)
        {
            if (_sharedContext != null)
            {
                _sharedContext.Players.Add(newPlayer);
                _sharedContext.SaveChanges();
            }
            else
            {
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    context.Players.Add(newPlayer);
                    context.SaveChanges();
                }
            }
        }
    }
}
