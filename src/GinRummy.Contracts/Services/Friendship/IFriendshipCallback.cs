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
        /// Delivers a change in the friendship between this player and another player: a request received, accepted or declined.
        /// </summary>
        /// <param name="friendshipEvent">What happened and with which player.</param>
        [OperationContract(IsOneWay = true)]
        void OnFriendshipEvent(FriendshipEvent friendshipEvent);
    }
}
