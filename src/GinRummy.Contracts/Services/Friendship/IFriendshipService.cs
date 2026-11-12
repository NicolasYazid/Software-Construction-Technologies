using System.ServiceModel;

using GinRummy.Contracts.DataContracts.Friendship;
using GinRummy.Contracts.Faults.Shared;

namespace GinRummy.Contracts.Services.Friendship
{
    /// <summary>
    /// Operations of the friendships between registered players. The client subscribes once and receives the
    /// friendship notices through IFriendshipCallback (CU-12).
    /// </summary>
    [ServiceContract(CallbackContract = typeof(IFriendshipCallback), SessionMode = SessionMode.Required)]
    public interface IFriendshipService
    {
        /// <summary>
        /// Subscribes the client to the friendship notices of the player who owns the access token.
        /// </summary>
        /// <param name="accessToken">Access token the server granted when the player signed in.</param>
        /// <exception cref="FaultException{ServiceFault}">
        /// Thrown with SessionExpired when the access token is altered, expired or belongs to a guest.
        /// </exception>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void SubscribeToFriendship(string accessToken);

        /// <summary>
        /// Sends a friend request from the player who owns the access token to the player with the public tag.
        /// The request is stored only when no rule rejects it.
        /// </summary>
        /// <param name="accessToken">Access token the server granted when the player signed in.</param>
        /// <param name="addresseePublicTag">Public tag of the player who receives the request.</param>
        /// <returns>Sent when the request was stored; otherwise, the rule that rejected it.</returns>
        /// <exception cref="FaultException{ServiceFault}">
        /// Thrown with SessionExpired when the access token is altered, expired or belongs to a guest.
        /// </exception>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        FriendRequestOutcome SendFriendRequest(string accessToken, int addresseePublicTag);

        /// <summary>
        /// Stops the friendship notices of this client. Calling it more than once has no effect.
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void UnsubscribeFromFriendship();
    }
}
