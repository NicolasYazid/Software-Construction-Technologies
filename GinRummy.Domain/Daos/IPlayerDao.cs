using GinRummy.Domain.Entities;

namespace GinRummy.Domain.Daos
{
    // Contract the game's logic uses to reach Player data.
    // The logic does not need to know how or where the data is stored.
    // Infrastructure provides the implementation.
    public interface IPlayerDao
    {
        Player FindByEmail(string email);

        void Add(Player newPlayer);
    }
}
