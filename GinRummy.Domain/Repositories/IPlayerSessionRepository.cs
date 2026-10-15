using System;

using GinRummy.Domain.Entities;

namespace GinRummy.Domain.Repositories
{
    /// <summary>
    /// Contract the game's logic uses to reach registered players' sessions, without knowing
    /// how or where they are stored. Guest sessions are added together with their entity.
    /// </summary>
    public interface IPlayerSessionRepository
    {
        /// <summary>
        /// Finds a session by its identifier, or null when it does not exist.
        /// </summary>
        /// <param name="playerSessionId">Identifier of the session.</param>
        /// <returns>The matching session, or null.</returns>
        PlayerSession FindById(int playerSessionId);

        /// <summary>
        /// Finds the session a player currently has open, or null when there is none. The
        /// database allows at most one open session per player.
        /// </summary>
        /// <param name="playerId">Player to look for.</param>
        /// <returns>The open session, or null.</returns>
        PlayerSession FindActiveByPlayerId(int playerId);

        /// <summary>
        /// Adds a new session and persists it immediately.
        /// </summary>
        /// <param name="newSession">Session to create.</param>
        void Add(PlayerSession newSession);

        /// <summary>
        /// Ends the given session at the given moment and persists the change.
        /// </summary>
        /// <param name="playerSessionId">Session to end.</param>
        /// <param name="endedAt">Moment the session ended, in UTC.</param>
        /// <exception cref="InvalidOperationException">Thrown when the session does not exist.</exception>
        void End(int playerSessionId, DateTime endedAt);
    }
}
