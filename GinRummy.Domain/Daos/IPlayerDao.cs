using GinRummy.Domain.Entities;

namespace GinRummy.Domain.Daos
{
    public interface IPlayerDao
    {
        Player FindByEmail(string email);

        void Add(Player newPlayer);
    }
}
