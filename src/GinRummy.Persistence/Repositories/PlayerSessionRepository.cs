using System;
using System.Linq;

using GinRummy.Domain.Entities;
using GinRummy.Domain.Repositories;
using GinRummy.Persistence.Persistence;

namespace GinRummy.Persistence.Repositories
{
    /// <summary>
    /// Fulfils IPlayerSessionRepository using Entity Framework against GinRummy_Dev. It always
    /// runs on the context of the unit of work that created it, so its changes share that
    /// unit's transaction.
    /// </summary>
    public class PlayerSessionRepository : IPlayerSessionRepository
    {
        private readonly GinRummyContext _context;

        /// <summary>
        /// Builds the repository on a context someone else opened. The caller stays responsible
        /// for disposing that context.
        /// </summary>
        /// <param name="context">Context to run every operation on.</param>
        public PlayerSessionRepository(GinRummyContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Finds a session by its identifier, or null when it does not exist.
        /// </summary>
        /// <param name="playerSessionId">Identifier of the session.</param>
        /// <returns>The matching session, or null.</returns>
        public PlayerSession FindById(int playerSessionId)
        {
            return _context.PlayerSessions.FirstOrDefault(session => session.PlayerSessionId == playerSessionId);
        }

        /// <summary>
        /// Finds the session a player currently has open, or null when there is none. The
        /// database allows at most one open session per player.
        /// </summary>
        /// <param name="playerId">Player to look for.</param>
        /// <returns>The open session, or null.</returns>
        public PlayerSession FindActiveByPlayerId(int playerId)
        {
            // The open state is read from EndedAt because IsActive is computed in C# and
            // cannot be translated to SQL.
            return _context.PlayerSessions
                .FirstOrDefault(session => (session.PlayerId == playerId) && (session.EndedAt == null));
        }

        /// <summary>
        /// Adds a new session and persists it immediately.
        /// </summary>
        /// <param name="newSession">Session to create.</param>
        public void Add(PlayerSession newSession)
        {
            _context.PlayerSessions.Add(newSession);
            _context.SaveChanges();
        }

        /// <summary>
        /// Ends the given session at the given moment and persists the change.
        /// </summary>
        /// <param name="playerSessionId">Session to end.</param>
        /// <param name="endedAt">Moment the session ended, in UTC.</param>
        /// <exception cref="InvalidOperationException">Thrown when the session does not exist.</exception>
        public void End(int playerSessionId, DateTime endedAt)
        {
            PlayerSession session = FindById(playerSessionId);
            if (session == null)
            {
                throw new InvalidOperationException("The player session does not exist.");
            }

            session.End(endedAt);
            _context.SaveChanges();
        }

        /// <summary>
        /// Finds a guest session by its identifier, or null when it does not exist.
        /// </summary>
        /// <param name="guestSessionId">Identifier of the guest session.</param>
        /// <returns>The matching guest session, or null.</returns>
        public GuestSession FindGuestById(int guestSessionId)
        {
            return _context.GuestSessions.FirstOrDefault(guest => guest.GuestSessionId == guestSessionId);
        }

        /// <summary>
        /// Adds a new guest session and persists it immediately.
        /// </summary>
        /// <param name="newGuest">Guest session to create.</param>
        public void AddGuest(GuestSession newGuest)
        {
            _context.GuestSessions.Add(newGuest);
            _context.SaveChanges();
        }
    }
}
