using System.Linq;

using GinRummy.Data.EntityFramework.Persistence;
using GinRummy.Domain.Daos;
using GinRummy.Domain.Entities;

namespace GinRummy.Data.EntityFramework.Daos
{
    public class PlayerDao : IPlayerDao
    {
        private readonly string _connectionStringName;
        private readonly GinRummyContext _sharedContext;

        public PlayerDao(string connectionStringName)
        {
            _connectionStringName = connectionStringName;
        }

        // The context can come from the caller so that several DAOs take part in one transaction.
        // Only the contexts this DAO creates are disposed of here; a shared one belongs to its caller.
        public PlayerDao(GinRummyContext sharedContext)
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
