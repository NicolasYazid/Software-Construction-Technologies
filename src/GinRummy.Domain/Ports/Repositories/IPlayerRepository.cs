using GinRummy.Domain.Entities.Players;

namespace GinRummy.Domain.Ports.Repositories
{
    /// <summary>
    /// Contract the game's logic uses to reach Player data, without knowing how or
    /// where it is stored. Infrastructure provides the implementation.
    /// </summary>
    public interface IPlayerRepository
    {
        /// <summary>
        /// Finds a player by its internal identifier, or null when it does not exist.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <returns>The matching player, or null.</returns>
        Player FindById(int playerId);

        /// <summary>
        /// Finds a player by the public tag other players see, or null when no player has it.
        /// </summary>
        /// <param name="publicTag">Public tag to search for.</param>
        /// <returns>The matching player, or null.</returns>
        Player FindByPublicTag(int publicTag);

        /// <summary>
        /// Adds a new player and persists it immediately, so its identifier is known
        /// right after the call.
        /// </summary>
        /// <param name="newPlayer">Player to create.</param>
        void Add(Player newPlayer);
    }
}
