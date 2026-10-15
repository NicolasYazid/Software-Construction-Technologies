using GinRummy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GinRummy.Domain.Repositories
{
    /// <summary>
    /// Contract the game's logic uses to reach PlayerAccount data, without knowing how or
    /// where it is stored. Infrastructure provides the implementation.
    /// </summary>
    public interface IPlayerAccountRepository
    {
        /// <summary>
        /// Finds the account whose email matches, with its player loaded, or null when no
        /// account has it.
        /// </summary>
        /// <param name="email">Email address to search for.</param>
        /// <returns>The matching account, or null.</returns>
        PlayerAccount FindByEmail(string email);

        /// <summary>
        /// Adds a new account and persists it immediately.
        /// </summary>
        /// <param name="newAccount">Account to create.</param>
        void Add(PlayerAccount newAccount);
    }
}
