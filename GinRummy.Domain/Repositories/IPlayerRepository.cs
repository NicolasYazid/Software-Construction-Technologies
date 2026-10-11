using GinRummy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GinRummy.Domain.Repositories
{
    /// <summary>
    /// Contract the game's logic uses to reach Player data, without knowing how or
    /// where it is stored. Infrastructure provides the implementation.
    /// </summary>
    public interface IPlayerRepository
    {
        /// <summary>
        /// Finds the player whose email matches, or null when no player has it.
        /// </summary>
        /// <param name="email">Email address to search for.</param>
        /// <returns>The matching player, or null.</returns>
        Player FindByEmail(string email);

        /// <summary>
        /// Adds a new player and persists it immediately.
        /// </summary>
        /// <param name="newPlayer">Player to create.</param>
        void Add(Player newPlayer);
    }
}
