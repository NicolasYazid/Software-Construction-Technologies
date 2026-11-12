using System.ServiceModel;

using GinRummy.Contracts.DataContracts.Friendship;

namespace GinRummy.Contracts.Services.Friendship
{
    /// <summary>
    /// Notices the server sends, without being asked, to a client subscribed to the friendship service.
    /// </summary>
    public interface IFriendshipCallback
    {
        /// <summary>
        /// Delivers a friend request that another player has just sent to this player (CU-12 step 9).
        /// </summary>
        /// <param name="request">Request received, already stored by the server.</param>
        [OperationContract(IsOneWay = true)]
        void OnFriendRequestReceived(FriendRequest request);
    }
}
