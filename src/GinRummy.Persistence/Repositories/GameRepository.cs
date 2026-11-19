using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using GinRummy.Domain.Entities.Gameplay;
using GinRummy.Domain.Ports.Gameplay;
using GinRummy.Persistence.Persistence;

namespace GinRummy.Persistence.Repositories
{
    public class GameRepository : IGameRepository
    {
        private readonly GinRummyContext _context;

        public GameRepository(GinRummyContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public GameSession FindById(int gameSessionId)
        {
            return _context.GameSessions
                .Include(gameSession => gameSession.Participants)
                .FirstOrDefault(gameSession => gameSession.GameSessionId == gameSessionId);
        }

        public GameSession FindGameSessionInProgressByPlayerAccountId(int playerAccountId)
        {
            string finished = GameState.Finished.ToString();

            return _context.GameSessions
                .Include(gameSession => gameSession.Participants)
                .Where(gameSession => gameSession.GameStateName != finished)
                .FirstOrDefault(gameSession => gameSession.Participants.Any(participant => participant.PlayerAccountId == playerAccountId));
        }

        public GameSession FindOpenByGuestSessionId(int guestSessionId)
        {
            string finished = GameState.Finished.ToString();

            return _context.GameSessions
                .Include(gameSession => gameSession.Participants)
                .Where(gameSession => gameSession.GameStateName != finished)
                .FirstOrDefault(gameSession => gameSession.Participants.Any(participant => participant.GuestSessionId == guestSessionId));
        }

        public GameSession FindActiveGameSessionByPlayerAccountId(int playerAccountId)
        {
            return QueryActiveGameSessions()
                .OrderByDescending(gameSession => gameSession.GameSessionId)
                .FirstOrDefault(gameSession => gameSession.Participants.Any(participant => participant.PlayerAccountId == playerAccountId));
        }

        public IList<GameSession> GetActiveGameSessions()
        {
            return QueryActiveGameSessions().ToList();
        }

        public void Add(GameSession newGameSession)
        {
            _context.GameSessions.Add(newGameSession);
            _context.SaveChanges();
        }

        public void AddParticipant(GameParticipant newParticipant)
        {
            _context.GameParticipants.Add(newParticipant);
            _context.SaveChanges();
        }

        public void AddHistory(GameHistory newEntry)
        {
            _context.GameHistories.Add(newEntry);
            _context.SaveChanges();
        }

        public IList<GameHistory> GetHistoryOf(int playerAccountId)
        {
            return _context.GameHistories
                .Include(history => history.Opponent.PlayerProfile)
                .Where(history => history.PlayerAccountId == playerAccountId)
                .OrderByDescending(history => history.PlayedAt)
                .ThenByDescending(history => history.MatchId)
                .ToList();
        }

        private IQueryable<GameSession> QueryActiveGameSessions()
        {
            string inProgress = GameState.InProgress.ToString();
            string paused = GameState.Paused.ToString();

            return _context.GameSessions
                .Include(gameSession => gameSession.Participants)
                .Where(gameSession => (gameSession.GameStateName == inProgress) || (gameSession.GameStateName == paused));
        }
    }
}
