using GinRummy.Domain.Entities.Authentication;

namespace GinRummy.Domain.Ports.Authentication
{
    public interface IGuestSessionRepository
    {
        GuestSession FindById(int guestSessionId);

        void Add(GuestSession newGuest);
    }
}
