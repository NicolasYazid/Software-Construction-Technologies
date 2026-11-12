using System.Collections.Generic;
using System.ServiceModel;

using GinRummy.Contracts.DataContracts.Friendship;
using GinRummy.Contracts.Faults.Shared;

namespace GinRummy.Contracts.Services.Friendship
{
    /// <summary>
    /// Operation to read the friend requests that a registered player has received and not answered yet (CU-13).
    /// </summary>
    [ServiceContract]
    public interface IPendingFriendRequestService
    {
        /// <summary>
        /// Gets the pending friend requests received by the player who owns the access token, from the oldest to the newest.
        /// </summary>
        /// <param name="accessToken">Access token the server granted when the player signed in.</param>
        /// <returns>The pending requests; empty when the player has none.</returns>
        /// <exception cref="FaultException{ServiceFault}">
        /// Thrown with SessionExpired when the access token is altered, expired or belongs to a guest.
        /// </exception>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        List<FriendRequest> GetPendingFriendRequests(string accessToken);
    }
}
