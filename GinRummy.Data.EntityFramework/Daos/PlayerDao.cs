using GinRummy.Data.EntityFramework.Persistence;
using GinRummy.Domain.Daos;
using GinRummy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GinRummy.Data.EntityFramework.Daos
{
    // Fulfils IPlayerDao using Entity Framework against GinRummy_Dev. By default, opens a
    // short-lived context per operation; optionally reuses a context someone else already
    // opened, so several DAOs can share one transaction.
    public class PlayerDao : IPlayerDao
    {
        private readonly string _connectionStringName;
        private readonly GinRummyContext _sharedContext;

        public PlayerDao(string connectionStringName)
        {
            _connectionStringName = connectionStringName;
        }

        public PlayerDao(GinRummyContext sharedContext)
        {
            _sharedContext = sharedContext;
        }

        public Player FindByEmail(string email)
        {
            Player foundPlayer;
            if (_sharedContext != null)
            {
                // Not wrapped in using: the caller opened this context and stays
                // responsible for closing it.
                foundPlayer = _sharedContext.Players.FirstOrDefault(player => player.Email == email);
            }
            else
            {
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    foundPlayer = context.Players.FirstOrDefault(player => player.Email == email);
                }
            }

            return foundPlayer;
        }

        public void Add(Player newPlayer)
        {
            if (_sharedContext != null)
            {
                _sharedContext.Players.Add(newPlayer);
                _sharedContext.SaveChanges();
            }
            else
            {
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    context.Players.Add(newPlayer);
                    context.SaveChanges();
                }
            }
        }
    }
}
