using System;
using System.Linq;

using GinRummy.Domain.Entities;
using GinRummy.Domain.Ports.Repositories;
using GinRummy.Persistence.Persistence;

namespace GinRummy.Persistence.Repositories
{
    /// <summary>
    /// Fulfils IGuestSessionRepository using Entity Framework against GinRummy_Dev. It always
    /// runs on the context of the unit of work that created it, so its changes share that
    /// unit's transaction.
    /// </summary>
    public class GuestSessionRepository : IGuestSessionRepository
    {
        private readonly GinRummyContext _context;

        /// <summary>
        /// Builds the repository on a context someone else opened. The caller stays responsible
        /// for disposing that context.
        /// </summary>
        /// <param name="context">Context to run every operation on.</param>
        public GuestSessionRepository(GinRummyContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Finds a guest session by its identifier, or null when it does not exist.
        /// </summary>
        /// <param name="guestSessionId">Identifier of the guest session.</param>
        /// <returns>The matching guest session, or null.</returns>
        public GuestSession FindById(int guestSessionId)
        {
            return _context.GuestSessions.FirstOrDefault(guest => guest.GuestSessionId == guestSessionId);
        }

        /// <summary>
        /// Adds a new guest session and persists it immediately.
        /// </summary>
        /// <param name="newGuest">Guest session to create.</param>
        public void Add(GuestSession newGuest)
        {
            _context.GuestSessions.Add(newGuest);
            _context.SaveChanges();
        }
    }
}
