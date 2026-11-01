using System.Collections.Generic;
using System.ServiceModel;

using GinRummy.Contracts.Callbacks;
using GinRummy.Contracts.DataContracts;

namespace GinRummy.Contracts.Services
{
    /// <summary>
    /// Duplex operations of the real-time presence of a player's friends (CU-11). The server
    /// pushes each change back through <see cref="IPlayerPresenceCallback"/>.
    /// </summary>
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IPlayerPresenceCallback))]
    public interface IPlayerPresenceService
    {
        /// <summary>
        /// Subscribes the player to the presence of their friends and gets the current state of each one.
        /// </summary>
        /// <param name="subscriberPublicTag">Public tag of the player that subscribes.</param>
        /// <returns>The presence of each friend; empty when the player has no friends.</returns>
        [OperationContract]
        List<FriendPresence> SubscribeToFriendsPresence(int subscriberPublicTag);

        /// <summary>
        /// Unsubscribes the player, so their friends see them go offline.
        /// </summary>
        /// <param name="subscriberPublicTag">Public tag of the player that unsubscribes.</param>
        [OperationContract(IsOneWay = true)]
        void UnsubscribeFromFriendsPresence(int subscriberPublicTag);
    }
}
