using System;
using System.Runtime.Serialization;

namespace GinRummy.Contracts.DataContracts.Friendship
{
    /// <summary>
    /// Pending friend request as the player who received it sees it.
    /// </summary>
    [DataContract]
    public class FriendRequest
    {
        /// <summary>
        /// Identifier of the request, which the client sends back to accept or reject it.
        /// </summary>
        [DataMember]
        public int FriendRequestId { get; set; }

        /// <summary>
        /// Username of the player who sent the request.
        /// </summary>
        [DataMember]
        public string RequesterUsername { get; set; }

        /// <summary>
        /// Public tag of the player who sent the request.
        /// </summary>
        [DataMember]
        public int RequesterPublicTag { get; set; }

        /// <summary>
        /// Moment the server stored the request, in UTC.
        /// </summary>
        [DataMember]
        public DateTime SentAt { get; set; }
    }
}
