using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using GinRummy.Domain.Entities.Friendship;
using GinRummy.Domain.Ports.Friendship;
using GinRummy.Persistence.Persistence;

namespace GinRummy.Persistence.Repositories
{
    /// <summary>
    /// Fulfils IFriendRepository using Entity Framework against GinRummy_Dev. It always runs on
    /// the context of the unit of work that created it, so its changes share that unit's
    /// transaction.
    /// </summary>
    public class FriendRepository : IFriendRepository
    {
        private readonly GinRummyContext _context;

        /// <summary>
        /// Builds the repository on a context someone else opened. The caller stays responsible
        /// for disposing that context.
        /// </summary>
        /// <param name="context">Context to run every operation on.</param>
        public FriendRepository(GinRummyContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Finds the request one player sent to another, or null when none is pending.
        /// </summary>
        /// <param name="requesterPlayerAccountId">Player who sent the request.</param>
        /// <param name="addresseePlayerAccountId">Player who received it.</param>
        /// <returns>The pending request, or null.</returns>
        public FriendRequest FindRequest(int requesterPlayerAccountId, int addresseePlayerAccountId)
        {
            return _context.FriendRequests
                .Where(request => request.RequesterPlayerAccountId == requesterPlayerAccountId)
                .FirstOrDefault(request => request.AddresseePlayerAccountId == addresseePlayerAccountId);
        }

        /// <summary>
        /// Finds a request by its identifier, or null when it does not exist.
        /// </summary>
        /// <param name="friendRequestId">Identifier of the request.</param>
        /// <returns>The matching request, or null.</returns>
        public FriendRequest FindRequestById(int friendRequestId)
        {
            return _context.FriendRequests.FirstOrDefault(request => request.FriendRequestId == friendRequestId);
        }

        /// <summary>
        /// Gets the pending requests a player has received, each with its requester loaded.
        /// </summary>
        /// <param name="addresseePlayerAccountId">Player who received the requests.</param>
        /// <returns>The pending requests, oldest first.</returns>
        public IList<FriendRequest> GetRequestsReceivedBy(int addresseePlayerAccountId)
        {
            // The requester and its player are loaded together because the caller needs the username of each requester to show the list.
            return _context.FriendRequests
                .Include(request => request.Requester.PlayerProfile)
                .Where(request => request.AddresseePlayerAccountId == addresseePlayerAccountId)
                .OrderBy(request => request.CreatedAt)
                .ToList();
        }

        /// <summary>
        /// Adds a new request and persists it immediately.
        /// </summary>
        /// <param name="newRequest">Request to create.</param>
        public void AddRequest(FriendRequest newRequest)
        {
            _context.FriendRequests.Add(newRequest);
            _context.SaveChanges();
        }

        /// <summary>
        /// Removes a request, whether it was accepted or rejected, and persists the change.
        /// Does nothing when the request no longer exists.
        /// </summary>
        /// <param name="friendRequestId">Request to remove.</param>
        public void RemoveRequest(int friendRequestId)
        {
            FriendRequest request = FindRequestById(friendRequestId);
            if (request != null)
            {
                _context.FriendRequests.Remove(request);
                _context.SaveChanges();
            }
        }

        /// <summary>
        /// Finds the friendship between two players, in any argument order, or null when they
        /// are not friends.
        /// </summary>
        /// <param name="firstPlayerAccountId">One of the players.</param>
        /// <param name="secondPlayerAccountId">The other player.</param>
        /// <returns>The friendship, or null.</returns>
        public FriendShip FindFriendShip(int firstPlayerAccountId, int secondPlayerAccountId)
        {
            // The pair is stored once with the lower id first, so the arguments are put in that order before searching.
            int lowerPlayerAccountId = Math.Min(firstPlayerAccountId, secondPlayerAccountId);
            int higherPlayerAccountId = Math.Max(firstPlayerAccountId, secondPlayerAccountId);

            return _context.FriendShips
                .Where(friendShip => friendShip.LowerPlayerAccountId == lowerPlayerAccountId)
                .FirstOrDefault(friendShip => friendShip.HigherPlayerAccountId == higherPlayerAccountId);
        }

        /// <summary>
        /// Gets every friendship a player takes part in, each with both players loaded.
        /// </summary>
        /// <param name="playerAccountId">Player whose friendships are wanted.</param>
        /// <returns>The friendships.</returns>
        public IList<FriendShip> GetFriendShipsOf(int playerAccountId)
        {
            return _context.FriendShips
                .Include(friendShip => friendShip.LowerPlayerAccount.PlayerProfile)
                .Include(friendShip => friendShip.HigherPlayerAccount.PlayerProfile)
                .Where(friendShip => (friendShip.LowerPlayerAccountId == playerAccountId) || (friendShip.HigherPlayerAccountId == playerAccountId))
                .ToList();
        }

        /// <summary>
        /// Adds a new friendship and persists it immediately.
        /// </summary>
        /// <param name="newFriendShip">Friendship to create.</param>
        public void AddFriendShip(FriendShip newFriendShip)
        {
            _context.FriendShips.Add(newFriendShip);
            _context.SaveChanges();
        }

        /// <summary>
        /// Removes the friendship between two players and persists the change. Does nothing
        /// when they are not friends.
        /// </summary>
        /// <param name="firstPlayerAccountId">One of the players.</param>
        /// <param name="secondPlayerAccountId">The other player.</param>
        public void RemoveFriendShip(int firstPlayerAccountId, int secondPlayerAccountId)
        {
            FriendShip friendShip = FindFriendShip(firstPlayerAccountId, secondPlayerAccountId);
            if (friendShip != null)
            {
                _context.FriendShips.Remove(friendShip);
                _context.SaveChanges();
            }
        }
    }
}
