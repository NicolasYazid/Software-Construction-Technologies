using System.Linq;

using GinRummy.Data.EntityFramework.Persistence;
using GinRummy.Domain.Daos;
using GinRummy.Domain.Entities;

namespace GinRummy.Data.EntityFramework.Daos
{
    // Fulfils IPlayerDao using Entity Framework against GinRummy_Dev.
    // By default, it opens a short-lived context per operation.
    // It can also reuse a context opened elsewhere, so several DAOs can share one transaction.
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
                // The caller owns this context.
                // It is disposed of when the caller's transaction ends.
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
