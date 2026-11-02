using System;
using System.Collections.Generic;
using System.Linq;

using GinRummy.Domain.Entities.Players;
using GinRummy.Domain.Ports.Repositories;
using GinRummy.Persistence.Persistence;

namespace GinRummy.Persistence.Repositories
{
    public class PlayerRepository : IPlayerRepository
    {
        private readonly string _connectionStringName;
        private readonly GinRummyContext _sharedContext;

        public PlayerRepository(string connectionStringName)
        {
            _connectionStringName = connectionStringName
                ?? throw new ArgumentNullException(nameof(connectionStringName));
        }

        public PlayerRepository(GinRummyContext sharedContext)
        {
            _sharedContext = sharedContext ?? throw new ArgumentNullException(nameof(sharedContext));
        }

        public Player FindById(int playerId)
        {
            Player foundPlayer;
            if (_sharedContext != null)
            {
                foundPlayer = _sharedContext.Players.FirstOrDefault(player => player.PlayerId == playerId);
            }
            else
            {
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    foundPlayer = context.Players.FirstOrDefault(player => player.PlayerId == playerId);
                }
            }

            return foundPlayer;
        }

        public Player FindByPublicTag(int publicTag)
        {
            Player foundPlayer;
            if (_sharedContext != null)
            {
                foundPlayer = _sharedContext.Players.FirstOrDefault(player => player.PublicTag == publicTag);
            }
            else
            {
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    foundPlayer = context.Players.FirstOrDefault(player => player.PublicTag == publicTag);
                }
            }

            return foundPlayer;
        }

        public IList<Player> GetAllPlayers()
        {
            IList<Player> allPlayers;
            if (_sharedContext != null)
            {
                allPlayers = _sharedContext.Players.ToList();
            }
            else
            {
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    allPlayers = context.Players.ToList();
                }
            }

            return allPlayers;
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
