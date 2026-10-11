using System.Linq;

using GinRummy.Data.EntityFramework.Persistence;
using GinRummy.Domain.Entities;
using GinRummy.Domain.Repositories;

namespace GinRummy.Data.EntityFramework.Repositories
{
    public class PlayerRepository : IPlayerRepository
    {
        private readonly string _connectionStringName;
        private readonly GinRummyContext _sharedContext;

        public PlayerRepository(string connectionStringName)
        {
            _connectionStringName = connectionStringName;
        }

        // The context can come from the caller so that several repositories take part in one transaction.
        // Only the contexts this repository creates are disposed of here; a shared one belongs to its caller.
        public PlayerRepository(GinRummyContext sharedContext)
        {
            _sharedContext = sharedContext;
        }

        public Player FindByEmail(string email)
        {
            Player foundPlayer;
            if (_sharedContext != null)
            {
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
