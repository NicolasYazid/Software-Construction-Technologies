using System;
using System.Runtime.Serialization;

namespace GinRummy.Contracts.DataContracts.Friendship
{
    [DataContract]
    public class FriendRequest
    {
        [DataMember]
        public int FriendRequestId { get; set; }

        [DataMember]
        public string RequesterUsername { get; set; }

        [DataMember]
        public int RequesterPublicTag { get; set; }

        [DataMember]
        public DateTime SentAt { get; set; }
    }
}
