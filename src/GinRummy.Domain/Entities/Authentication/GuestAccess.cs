using System;

namespace GinRummy.Domain.Entities.Authentication
{
    public class GuestAccess
    {
        public GuestAccess(GuestSession guest, AccessToken accessToken)
        {
            Guest = guest ?? throw new ArgumentNullException(nameof(guest));
            AccessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
        }

        public GuestSession Guest { get; private set; }

        public AccessToken AccessToken { get; private set; }
    }
}
