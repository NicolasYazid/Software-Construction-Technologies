using System;
using System.Runtime.Serialization;

namespace GinRummy.Contracts.DataContracts.Friendship
{
    /// <summary>
    /// Notice of a change in the friendship between the player who receives it and another player.
    /// </summary>
    [DataContract]
    public class FriendshipEvent
    {
        /// <summary>
        /// What happened.
        /// </summary>
        [DataMember]
        public FriendshipEventType EventType { get; set; }

        /// <summary>
        /// Public tag of the other player, which the client uses to find that player in its lists.
        /// </summary>
        [DataMember]
        public int OtherPlayerPublicTag { get; set; }

        /// <summary>
        /// Identifier of the request received; only filled when the event type is FriendRequestReceived.
        /// </summary>
        [DataMember]
        public int FriendRequestId { get; set; }

        /// <summary>
        /// Username of the other player; only filled when the event type is FriendRequestReceived.
        /// </summary>
        [DataMember]
        public string OtherPlayerUsername { get; set; }

        /// <summary>
        /// Moment the server stored the request, in UTC; only filled when the event type is FriendRequestReceived.
        /// </summary>
        [DataMember]
        public DateTime SentAt { get; set; }
    }
}
