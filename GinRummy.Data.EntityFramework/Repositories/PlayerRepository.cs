using System.Linq;

using GinRummy.Data.EntityFramework.Persistence;
using GinRummy.Domain.Entities;
using GinRummy.Domain.Repositories;

namespace GinRummy.Data.EntityFramework.Repositories
{
    /// <summary>
    /// Fulfils IPlayerRepository using Entity Framework against GinRummy_Dev. Opens
    /// a short-lived context per operation, per the team's mandatory use of "using"
    /// for disposable resources.
    /// </summary>
    public class PlayerRepository : IPlayerRepository
    {
        private readonly string _connectionStringName;

        /// <summary>
        /// Builds the repository against the given connection string entry.
        /// </summary>
        /// <param name="connectionStringName">Name of the entry in App.config.</param>
        public PlayerRepository(string connectionStringName)
        {
            _connectionStringName = connectionStringName;
        }

        /// <summary>
        /// Finds the player whose email matches, or null when no player has it.
        /// </summary>
        /// <param name="email">Email address to search for.</param>
        /// <returns>The matching player, or null.</returns>
        public Player FindByEmail(string email)
        {
            Player foundPlayer = null;
            using (GinRummyContext context = new GinRummyContext(_connectionStringName))
            {
                foundPlayer = context.Players.FirstOrDefault(player => player.Email == email);
            }

            return foundPlayer;
        }

        /// <summary>
        /// Adds a new player and persists it immediately.
        /// </summary>
        /// <param name="newPlayer">Player to create.</param>
        public void Add(Player newPlayer)
        {
            using (GinRummyContext context = new GinRummyContext(_connectionStringName))
            {
                context.Players.Add(newPlayer);
                context.SaveChanges();
            }
        }
    }
}
