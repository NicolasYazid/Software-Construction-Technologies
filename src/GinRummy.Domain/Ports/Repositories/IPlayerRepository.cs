using System.Collections.Generic;
using GinRummy.Domain.Entities.Players;

namespace GinRummy.Domain.Ports.Repositories
{
    public interface IPlayerRepository
    {
        Player FindById(int playerId);

        Player FindByPublicTag(int publicTag);

        IList<Player> GetAllPlayers();

        void Add(Player newPlayer);
    }
}
