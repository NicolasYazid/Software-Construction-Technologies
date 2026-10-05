using GinRummy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GinRummy.Domain.Daos
{
    // Contract the game's logic uses to reach Player data, without knowing how or where it is
    // stored. Infrastructure provides the implementation.
    public interface IPlayerDao
    {
        Player FindByEmail(string email);

        void Add(Player newPlayer);
    }
}
