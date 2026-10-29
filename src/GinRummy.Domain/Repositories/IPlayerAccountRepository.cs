using GinRummy.Domain.Entities;

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
        /// Finds the account of a player, with its player loaded, or null when the player has
        /// no account.
        /// </summary>
        /// <param name="playerId">Identifier of the player that owns the account.</param>
        /// <returns>The matching account, or null.</returns>
        PlayerAccount FindByPlayerId(int playerId);

        /// <summary>
        /// Adds a new account and persists it immediately.
        /// </summary>
        /// <param name="newAccount">Account to create.</param>
        void Add(PlayerAccount newAccount);
    }
}
