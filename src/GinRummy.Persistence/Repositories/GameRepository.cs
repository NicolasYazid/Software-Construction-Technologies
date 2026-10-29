using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using GinRummy.Domain.Entities;
using GinRummy.Domain.Repositories;
using GinRummy.Persistence.Persistence;

namespace GinRummy.Persistence.Repositories
{
    /// <summary>
    /// Fulfils IGameRepository using Entity Framework against GinRummy_Dev. It always runs on
    /// the context of the unit of work that created it, so its changes share that unit's
    /// transaction.
    /// </summary>
    public class GameRepository : IGameRepository
    {
        private readonly GinRummyContext _context;

        /// <summary>
        /// Builds the repository on a context someone else opened. The caller stays responsible
        /// for disposing that context.
        /// </summary>
        /// <param name="context">Context to run every operation on.</param>
        public GameRepository(GinRummyContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Finds a match with its participants, or null when it does not exist.
        /// </summary>
        /// <param name="gameSessionId">Identifier of the match.</param>
        /// <returns>The matching match, or null.</returns>
        public GameSession FindById(int gameSessionId)
        {
            return _context.GameSessions
                .Include(gameSession => gameSession.Participants)
                .FirstOrDefault(gameSession => gameSession.GameSessionId == gameSessionId);
        }

        /// <summary>
        /// Finds the match a registered player takes part in that is not over, or null when
        /// there is none.
        /// </summary>
        /// <param name="playerId">Player to look for.</param>
        /// <returns>The open match with its participants, or null.</returns>
        public GameSession FindOpenByPlayerId(int playerId)
        {
            // The stage is turned into text before the query because EF cannot translate the computed State property to SQL.
            string finished = GameState.Finished.ToString();

            return _context.GameSessions
                .Include(gameSession => gameSession.Participants)
                .Where(gameSession => gameSession.GameStateName != finished)
                .FirstOrDefault(gameSession => gameSession.Participants.Any(participant => participant.PlayerId == playerId));
        }

        /// <summary>
        /// Finds the match a guest takes part in that is not over, or null when there is none.
        /// It is what lets a guest return to the table after losing the connection.
        /// </summary>
        /// <param name="guestSessionId">Guest session to look for.</param>
        /// <returns>The open match with its participants, or null.</returns>
        public GameSession FindOpenByGuestSessionId(int guestSessionId)
        {
            string finished = GameState.Finished.ToString();

            return _context.GameSessions
                .Include(gameSession => gameSession.Participants)
                .Where(gameSession => gameSession.GameStateName != finished)
                .FirstOrDefault(gameSession => gameSession.Participants.Any(participant => participant.GuestSessionId == guestSessionId));
        }

        /// <summary>
        /// Adds a new match and persists it immediately.
        /// </summary>
        /// <param name="newGameSession">Match to create.</param>
        public void Add(GameSession newGameSession)
        {
            _context.GameSessions.Add(newGameSession);
            _context.SaveChanges();
        }

        /// <summary>
        /// Adds a participant to a match and persists it immediately.
        /// </summary>
        /// <param name="newParticipant">Participant to create.</param>
        public void AddParticipant(GameParticipant newParticipant)
        {
            _context.GameParticipants.Add(newParticipant);
            _context.SaveChanges();
        }

        /// <summary>
        /// Adds a history entry and persists it immediately.
        /// </summary>
        /// <param name="newEntry">Entry to create.</param>
        public void AddHistory(GameHistory newEntry)
        {
            _context.GameHistories.Add(newEntry);
            _context.SaveChanges();
        }

        /// <summary>
        /// Gets the history of a player, newest first.
        /// </summary>
        /// <param name="playerId">Player whose history is read.</param>
        /// <returns>The entries of the player; empty when there are none.</returns>
        public IList<GameHistory> GetHistoryOf(int playerId)
        {
            // The opponent and its player are loaded together because the caller shows the username of each opponent.
            return _context.GameHistories
                .Include(history => history.Opponent.Player)
                .Where(history => history.PlayerId == playerId)
                .OrderByDescending(history => history.PlayedAt)
                .ThenByDescending(history => history.MatchId)
                .ToList();
        }
    }
}
