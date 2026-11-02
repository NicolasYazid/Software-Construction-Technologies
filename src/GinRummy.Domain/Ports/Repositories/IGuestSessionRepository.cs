using GinRummy.Domain.Entities;

namespace GinRummy.Domain.Ports.Repositories
{
    /// <summary>
    /// Contract the game's logic uses to reach the sessions of guests, without knowing how or where they
    /// are stored. A registered player has no stored session: the access token proves who they are.
    /// </summary>
    public interface IGuestSessionRepository
    {
        /// <summary>
        /// Finds a guest session by its identifier, or null when it does not exist.
        /// </summary>
        /// <param name="guestSessionId">Identifier of the guest session.</param>
        /// <returns>The matching guest session, or null.</returns>
        GuestSession FindById(int guestSessionId);

        /// <summary>
        /// Adds a new guest session and persists it immediately.
        /// </summary>
        /// <param name="newGuest">Guest session to create.</param>
        void Add(GuestSession newGuest);
    }
}
