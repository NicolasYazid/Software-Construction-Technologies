using System.Collections.Generic;

using GinRummy.Domain.Entities.Friendship;

namespace GinRummy.Domain.Ports.Repositories
{
    /// <summary>
    /// Contract the game's logic uses to reach friend requests and friendships, without
    /// knowing how or where they are stored.
    /// </summary>
    public interface IFriendRepository
    {
        /// <summary>
        /// Finds the request one player sent to another, or null when none is pending.
        /// </summary>
        /// <param name="requesterPlayerId">Player who sent the request.</param>
        /// <param name="recipientPlayerId">Player who received it.</param>
        /// <returns>The pending request, or null.</returns>
        FriendRequest FindRequest(int requesterPlayerId, int recipientPlayerId);

        /// <summary>
        /// Finds a request by its identifier, or null when it does not exist.
        /// </summary>
        /// <param name="friendRequestId">Identifier of the request.</param>
        /// <returns>The matching request, or null.</returns>
        FriendRequest FindRequestById(int friendRequestId);

        /// <summary>
        /// Gets the pending requests a player has received, each with its requester loaded.
        /// </summary>
        /// <param name="recipientPlayerId">Player who received the requests.</param>
        /// <returns>The pending requests, oldest first.</returns>
        IList<FriendRequest> GetRequestsReceivedBy(int recipientPlayerId);

        /// <summary>
        /// Adds a new request and persists it immediately.
        /// </summary>
        /// <param name="newRequest">Request to create.</param>
        void AddRequest(FriendRequest newRequest);

        /// <summary>
        /// Removes a request, whether it was accepted or rejected, and persists the change.
        /// </summary>
        /// <param name="friendRequestId">Request to remove.</param>
        void RemoveRequest(int friendRequestId);

        /// <summary>
        /// Finds the friendship between two players, in any argument order, or null when they
        /// are not friends.
        /// </summary>
        /// <param name="firstPlayerId">One of the players.</param>
        /// <param name="secondPlayerId">The other player.</param>
        /// <returns>The friendship, or null.</returns>
        FriendShip FindFriendShip(int firstPlayerId, int secondPlayerId);

        /// <summary>
        /// Gets every friendship a player takes part in.
        /// </summary>
        /// <param name="playerId">Player whose friendships are wanted.</param>
        /// <returns>The friendships.</returns>
        IList<FriendShip> GetFriendShipsOf(int playerId);

        /// <summary>
        /// Adds a new friendship and persists it immediately.
        /// </summary>
        /// <param name="newFriendShip">Friendship to create.</param>
        void AddFriendShip(FriendShip newFriendShip);

        /// <summary>
        /// Removes the friendship between two players and persists the change.
        /// </summary>
        /// <param name="firstPlayerId">One of the players.</param>
        /// <param name="secondPlayerId">The other player.</param>
        void RemoveFriendShip(int firstPlayerId, int secondPlayerId);
    }
}
