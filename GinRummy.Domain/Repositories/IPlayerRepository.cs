using GinRummy.Domain.Entities;

namespace GinRummy.Domain.Repositories
{
    public interface IPlayerRepository
    {
        Player FindByEmail(string email);

        void Add(Player newPlayer);
    }
}
