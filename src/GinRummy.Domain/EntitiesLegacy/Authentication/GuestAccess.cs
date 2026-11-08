using System;

using GinRummy.Domain.Entities.Players;

namespace GinRummy.Domain.Entities.Authentication
{
    /// <summary>
    /// Outcome of entering as a guest: the session that was created and the access token that identifies it
    /// (CU-02 FA-01).
    /// </summary>
    public class GuestAccess
    {
        /// <summary>
        /// Builds the outcome of entering as a guest.
        /// </summary>
        /// <param name="guest">Session of the guest, already stored.</param>
        /// <param name="accessToken">Access token that identifies the guest.</param>
        /// <exception cref="ArgumentNullException">Thrown when the session or the token is null.</exception>
        public GuestAccess(GuestSession guest, AccessToken accessToken)
        {
            Guest = guest ?? throw new ArgumentNullException(nameof(guest));
            AccessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
        }

        /// <summary>
        /// Gets the session of the guest.
        /// </summary>
        public GuestSession Guest { get; private set; }

        /// <summary>
        /// Gets the access token that identifies the guest.
        /// </summary>
        public AccessToken AccessToken { get; private set; }
    }
}
