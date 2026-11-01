using System.ServiceModel;

using GinRummy.Contracts.DataContracts;

namespace GinRummy.Contracts.Callbacks
{
    /// <summary>
    /// Callback the server uses to push one friend's presence change to a subscribed client.
    /// </summary>
    public interface IPlayerPresenceCallback
    {
        /// <summary>
        /// Notifies the client that one of their friends changed presence.
        /// </summary>
        /// <param name="presence">The friend and their new presence state.</param>
        [OperationContract(IsOneWay = true)]
        void OnFriendPresenceChanged(FriendPresence presence);
    }
}
