using System.ServiceModel;

using GinRummy.Contracts.DataContracts.Friendship;
using GinRummy.Contracts.Faults.Shared;

namespace GinRummy.Contracts.Services.Friendship
{
    /// <summary>
    /// Operations to accept or decline a friend request that a registered player received (CU-14 and CU-15).
    /// </summary>
    [ServiceContract]
    public interface IFriendRequestResponseService
    {
        /// <summary>
        /// Accepts a pending friend request received by the player who owns the access token.
        /// </summary>
        /// <param name="accessToken">Access token the server granted when the player signed in.</param>
        /// <param name="friendRequestId">Identifier of the request to accept.</param>
        /// <returns>Accepted, or the reason the friendship was not created.</returns>
        /// <exception cref="FaultException{ServiceFault}">
        /// Thrown with SessionExpired when the access token is altered, expired or belongs to a guest.
        /// </exception>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        FriendRequestResponseOutcome AcceptFriendRequest(string accessToken, int friendRequestId);

        /// <summary>
        /// Declines a pending friend request received by the player who owns the access token.
        /// </summary>
        /// <param name="accessToken">Access token the server granted when the player signed in.</param>
        /// <param name="friendRequestId">Identifier of the request to decline.</param>
        /// <returns>Declined, or RequestNotFound when nothing changed.</returns>
        /// <exception cref="FaultException{ServiceFault}">
        /// Thrown with SessionExpired when the access token is altered, expired or belongs to a guest.
        /// </exception>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        FriendRequestResponseOutcome DeclineFriendRequest(string accessToken, int friendRequestId);
    }
}
