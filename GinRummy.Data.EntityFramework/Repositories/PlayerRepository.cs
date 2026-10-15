using GinRummy.Data.EntityFramework.Persistence;
using GinRummy.Domain.Entities;
using GinRummy.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GinRummy.Data.EntityFramework.Repositories
{
    /// <summary>
    /// Fulfils IPlayerRepository using Entity Framework against GinRummy_Dev. By default,
    /// opens a short-lived context per operation; optionally reuses a context someone else
    /// already opened, so several repositories can share one transaction.
    /// </summary>
    public class PlayerRepository : IPlayerRepository
    {
        private readonly string _connectionStringName;
        private readonly GinRummyContext _sharedContext;

        /// <summary>
        /// Builds the repository so that each operation opens and closes its own context.
        /// </summary>
        /// <param name="connectionStringName">Name of the entry in App.config.</param>
        public PlayerRepository(string connectionStringName)
        {
            _connectionStringName = connectionStringName;
        }

        /// <summary>
        /// Builds the repository so that every operation runs on a context someone else
        /// already opened, so it can share a transaction with other repositories. The
        /// caller stays responsible for disposing that context.
        /// </summary>
        /// <param name="sharedContext">Context to reuse instead of opening a new one.</param>
        public PlayerRepository(GinRummyContext sharedContext)
        {
            _sharedContext = sharedContext;
        }

        /// <summary>
        /// Adds a new player and persists it immediately, so its identifier is known
        /// right after the call.
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
