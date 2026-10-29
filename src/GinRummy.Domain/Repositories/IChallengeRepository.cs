using System.Collections.Generic;

using GinRummy.Domain.Entities;

namespace GinRummy.Domain.Repositories
{
    /// <summary>
    /// Contract the game's logic uses to reach direct challenges between players, without
    /// knowing how or where they are stored.
    /// </summary>
    public interface IChallengeRepository
    {
        /// <summary>
        /// Finds a challenge by its identifier, or null when it does not exist.
        /// </summary>
        /// <param name="challengeId">Identifier of the challenge.</param>
        /// <returns>The matching challenge, or null.</returns>
        Challenge FindById(int challengeId);

        /// <summary>
        /// Gets the challenges a player received that still wait for an answer, oldest first.
        /// </summary>
        /// <param name="challengedPlayerId">Player who received the challenges.</param>
        /// <returns>The pending challenges; empty when there are none.</returns>
        IList<Challenge> GetPendingReceivedBy(int challengedPlayerId);

        /// <summary>
        /// Gets the challenges a player sent that still wait for an answer, oldest first.
        /// </summary>
        /// <param name="challengerPlayerId">Player who sent the challenges.</param>
        /// <returns>The pending challenges; empty when there are none.</returns>
        IList<Challenge> GetPendingSentBy(int challengerPlayerId);

        /// <summary>
        /// Adds a new challenge and persists it immediately.
        /// </summary>
        /// <param name="newChallenge">Challenge to create.</param>
        void Add(Challenge newChallenge);
    }
}
