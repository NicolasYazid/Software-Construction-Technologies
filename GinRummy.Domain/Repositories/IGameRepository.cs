using System.Collections.Generic;

using GinRummy.Domain.Entities;

namespace GinRummy.Domain.Repositories
{
    /// <summary>
    /// Contract the game's logic uses to reach matches, their participants and the history of
    /// finished matches, without knowing how or where they are stored.
    /// </summary>
    public interface IGameRepository
    {
        /// <summary>
        /// Finds a match with its participants, or null when it does not exist.
        /// </summary>
        /// <param name="gameSessionId">Identifier of the match.</param>
        /// <returns>The matching match, or null.</returns>
        GameSession FindById(int gameSessionId);

        /// <summary>
        /// Finds the match a registered player takes part in that is not over, or null when
        /// there is none.
        /// </summary>
        /// <param name="playerId">Player to look for.</param>
        /// <returns>The open match with its participants, or null.</returns>
        GameSession FindOpenByPlayerId(int playerId);

        /// <summary>
        /// Adds a new match and persists it immediately.
        /// </summary>
        /// <param name="newGameSession">Match to create.</param>
        void Add(GameSession newGameSession);

        /// <summary>
        /// Adds a participant to a match and persists it immediately.
        /// </summary>
        /// <param name="newParticipant">Participant to create.</param>
        void AddParticipant(GameParticipant newParticipant);

        /// <summary>
        /// Adds a history entry and persists it immediately.
        /// </summary>
        /// <param name="newEntry">Entry to create.</param>
        void AddHistory(GameHistory newEntry);

        /// <summary>
        /// Gets the history of a player, newest first.
        /// </summary>
        /// <param name="playerId">Player whose history is read.</param>
        /// <returns>The entries of the player; empty when there are none.</returns>
        IList<GameHistory> GetHistoryOf(int playerId);
    }
}
