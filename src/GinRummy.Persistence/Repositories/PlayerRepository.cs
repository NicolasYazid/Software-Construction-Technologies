using System;
using System.Linq;

using GinRummy.Domain.Entities;
using GinRummy.Domain.Ports.Repositories;
using GinRummy.Persistence.Persistence;

namespace GinRummy.Persistence.Repositories
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
        /// <exception cref="ArgumentNullException">Thrown when the name is null.</exception>
        public PlayerRepository(string connectionStringName)
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
        public PlayerRepository(GinRummyContext sharedContext)
        {
            _sharedContext = sharedContext ?? throw new ArgumentNullException(nameof(sharedContext));
        }

        /// <summary>
        /// Finds a player by its internal identifier, or null when it does not exist.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <returns>The matching player, or null.</returns>
        public Player FindById(int playerId)
        {
            Player foundPlayer;
            if (_sharedContext != null)
            {
                foundPlayer = _sharedContext.Players.FirstOrDefault(player => player.PlayerId == playerId);
            }
            else
            {
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    foundPlayer = context.Players.FirstOrDefault(player => player.PlayerId == playerId);
                }
            }

            return foundPlayer;
        }

        /// <summary>
        /// Finds a player by the public tag other players see, or null when no player has it.
        /// </summary>
        /// <param name="publicTag">Public tag to search for.</param>
        /// <returns>The matching player, or null.</returns>
        public Player FindByPublicTag(int publicTag)
        {
            Player foundPlayer;
            if (_sharedContext != null)
            {
                foundPlayer = _sharedContext.Players.FirstOrDefault(player => player.PublicTag == publicTag);
            }
            else
            {
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    foundPlayer = context.Players.FirstOrDefault(player => player.PublicTag == publicTag);
                }
            }

            return foundPlayer;
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
