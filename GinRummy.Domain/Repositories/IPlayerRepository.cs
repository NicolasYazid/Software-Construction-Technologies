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
        /// Adds a new player and persists it immediately, so its identifier is known
        /// right after the call.
        /// </summary>
        /// <param name="newPlayer">Player to create.</param>
        void Add(Player newPlayer);
    }
}
