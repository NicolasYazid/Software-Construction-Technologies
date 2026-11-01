using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using GinRummy.Domain.Entities;
using GinRummy.Domain.Repositories;
using GinRummy.Persistence.Persistence;

namespace GinRummy.Persistence.Repositories
{
    /// <summary>
    /// Fulfils IFriendRepository using Entity Framework against GinRummy_Dev. By default, opens a
    /// short-lived context per operation; optionally reuses a context someone else already opened,
    /// so several repositories can share one transaction.
    /// </summary>
    public class FriendRepository : IFriendRepository
    {
        private readonly string _connectionStringName;
        private readonly GinRummyContext _sharedContext;

        /// <summary>
        /// Builds the repository so that each operation opens and closes its own context.
        /// </summary>
        /// <param name="connectionStringName">Name of the entry in App.config.</param>
        /// <exception cref="ArgumentNullException">Thrown when the name is null.</exception>
        public FriendRepository(string connectionStringName)
        {
            _connectionStringName = connectionStringName
                ?? throw new ArgumentNullException(nameof(connectionStringName));
        }

        /// <summary>
        /// Builds the repository on a context someone else opened. The caller stays responsible
        /// for disposing that context.
        /// </summary>
        /// <param name="sharedContext">Context to run every operation on.</param>
        /// <exception cref="ArgumentNullException">Thrown when the context is null.</exception>
        public FriendRepository(GinRummyContext sharedContext)
        {
            _sharedContext = sharedContext ?? throw new ArgumentNullException(nameof(sharedContext));
        }

        /// <summary>
        /// Finds the request one player sent to another, or null when none is pending.
        /// </summary>
        /// <param name="requesterPlayerId">Player who sent the request.</param>
        /// <param name="recipientPlayerId">Player who received it.</param>
        /// <returns>The pending request, or null.</returns>
        public FriendRequest FindRequest(int requesterPlayerId, int recipientPlayerId)
        {
            FriendRequest foundRequest;
            if (_sharedContext != null)
            {
                foundRequest = QueryRequest(_sharedContext, requesterPlayerId, recipientPlayerId);
            }
            else
            {
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    foundRequest = QueryRequest(context, requesterPlayerId, recipientPlayerId);
                }
            }

            return foundRequest;
        }

        /// <summary>
        /// Finds a request by its identifier, or null when it does not exist.
        /// </summary>
        /// <param name="friendRequestId">Identifier of the request.</param>
        /// <returns>The matching request, or null.</returns>
        public FriendRequest FindRequestById(int friendRequestId)
        {
            FriendRequest foundRequest;
            if (_sharedContext != null)
            {
                foundRequest = QueryRequestById(_sharedContext, friendRequestId);
            }
            else
            {
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    foundRequest = QueryRequestById(context, friendRequestId);
                }
            }

            return foundRequest;
        }

        /// <summary>
        /// Gets the pending requests a player has received, each with its requester loaded.
        /// </summary>
        /// <param name="recipientPlayerId">Player who received the requests.</param>
        /// <returns>The pending requests, oldest first.</returns>
        public IList<FriendRequest> GetRequestsReceivedBy(int recipientPlayerId)
        {
            IList<FriendRequest> requests;
            if (_sharedContext != null)
            {
                requests = QueryRequestsReceivedBy(_sharedContext, recipientPlayerId);
            }
            else
            {
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    requests = QueryRequestsReceivedBy(context, recipientPlayerId);
                }
            }

            return requests;
        }

        /// <summary>
        /// Adds a new request and persists it immediately.
        /// </summary>
        /// <param name="newRequest">Request to create.</param>
        public void AddRequest(FriendRequest newRequest)
        {
            if (_sharedContext != null)
            {
                InsertRequest(_sharedContext, newRequest);
            }
            else
            {
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    InsertRequest(context, newRequest);
                }
            }
        }

        /// <summary>
        /// Removes a request, whether it was accepted or rejected, and persists the change.
        /// Does nothing when the request no longer exists.
        /// </summary>
        /// <param name="friendRequestId">Request to remove.</param>
        public void RemoveRequest(int friendRequestId)
        {
            if (_sharedContext != null)
            {
                DeleteRequest(_sharedContext, friendRequestId);
            }
            else
            {
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    DeleteRequest(context, friendRequestId);
                }
            }
        }

        /// <summary>
        /// Finds the friendship between two players, in any argument order, or null when they
        /// are not friends.
        /// </summary>
        /// <param name="firstPlayerId">One of the players.</param>
        /// <param name="secondPlayerId">The other player.</param>
        /// <returns>The friendship, or null.</returns>
        public FriendShip FindFriendShip(int firstPlayerId, int secondPlayerId)
        {
            FriendShip foundFriendShip;
            if (_sharedContext != null)
            {
                foundFriendShip = QueryFriendShip(_sharedContext, firstPlayerId, secondPlayerId);
            }
            else
            {
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    foundFriendShip = QueryFriendShip(context, firstPlayerId, secondPlayerId);
                }
            }

            return foundFriendShip;
        }

        /// <summary>
        /// Gets every friendship a player takes part in, each with both players loaded.
        /// </summary>
        /// <param name="playerId">Player whose friendships are wanted.</param>
        /// <returns>The friendships.</returns>
        public IList<FriendShip> GetFriendShipsOf(int playerId)
        {
            IList<FriendShip> friendShips;
            if (_sharedContext != null)
            {
                friendShips = QueryFriendShipsOf(_sharedContext, playerId);
            }
            else
            {
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    friendShips = QueryFriendShipsOf(context, playerId);
                }
            }

            return friendShips;
        }

        /// <summary>
        /// Adds a new friendship and persists it immediately.
        /// </summary>
        /// <param name="newFriendShip">Friendship to create.</param>
        public void AddFriendShip(FriendShip newFriendShip)
        {
            if (_sharedContext != null)
            {
                InsertFriendShip(_sharedContext, newFriendShip);
            }
            else
            {
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    InsertFriendShip(context, newFriendShip);
                }
            }
        }

        /// <summary>
        /// Removes the friendship between two players and persists the change. Does nothing
        /// when they are not friends.
        /// </summary>
        /// <param name="firstPlayerId">One of the players.</param>
        /// <param name="secondPlayerId">The other player.</param>
        public void RemoveFriendShip(int firstPlayerId, int secondPlayerId)
        {
            if (_sharedContext != null)
            {
                DeleteFriendShip(_sharedContext, firstPlayerId, secondPlayerId);
            }
            else
            {
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    DeleteFriendShip(context, firstPlayerId, secondPlayerId);
                }
            }
        }

        private static FriendRequest QueryRequest(GinRummyContext context, int requesterPlayerId, int recipientPlayerId)
        {
            return context.FriendRequests
                .Where(request => request.RequesterPlayerId == requesterPlayerId)
                .FirstOrDefault(request => request.RecipientPlayerId == recipientPlayerId);
        }

        private static FriendRequest QueryRequestById(GinRummyContext context, int friendRequestId)
        {
            return context.FriendRequests.FirstOrDefault(request => request.FriendRequestId == friendRequestId);
        }

        private static IList<FriendRequest> QueryRequestsReceivedBy(GinRummyContext context, int recipientPlayerId)
        {
            // The requester and its player are loaded together because the caller needs the username of each requester to show the list.
            return context.FriendRequests
                .Include(request => request.Requester.Player)
                .Where(request => request.RecipientPlayerId == recipientPlayerId)
                .OrderBy(request => request.CreatedAt)
                .ToList();
        }

        private static void InsertRequest(GinRummyContext context, FriendRequest newRequest)
        {
            context.FriendRequests.Add(newRequest);
            context.SaveChanges();
        }

        private static void DeleteRequest(GinRummyContext context, int friendRequestId)
        {
            FriendRequest request = QueryRequestById(context, friendRequestId);
            if (request != null)
            {
                context.FriendRequests.Remove(request);
                context.SaveChanges();
            }
        }

        private static FriendShip QueryFriendShip(GinRummyContext context, int firstPlayerId, int secondPlayerId)
        {
            // The pair is stored once with the lower id first, so the arguments are put in that order before searching.
            int playerAId = Math.Min(firstPlayerId, secondPlayerId);
            int playerBId = Math.Max(firstPlayerId, secondPlayerId);

            return context.FriendShips
                .Where(friendShip => friendShip.PlayerAId == playerAId)
                .FirstOrDefault(friendShip => friendShip.PlayerBId == playerBId);
        }

        private static IList<FriendShip> QueryFriendShipsOf(GinRummyContext context, int playerId)
        {
            return context.FriendShips
                .Include(friendShip => friendShip.PlayerA.Player)
                .Include(friendShip => friendShip.PlayerB.Player)
                .Where(friendShip => (friendShip.PlayerAId == playerId) || (friendShip.PlayerBId == playerId))
                .ToList();
        }

        private static void InsertFriendShip(GinRummyContext context, FriendShip newFriendShip)
        {
            context.FriendShips.Add(newFriendShip);
            context.SaveChanges();
        }

        private static void DeleteFriendShip(GinRummyContext context, int firstPlayerId, int secondPlayerId)
        {
            FriendShip friendShip = QueryFriendShip(context, firstPlayerId, secondPlayerId);
            if (friendShip != null)
            {
                context.FriendShips.Remove(friendShip);
                context.SaveChanges();
            }
        }
    }
}
