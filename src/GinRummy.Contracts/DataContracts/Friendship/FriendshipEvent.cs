using System;
using System.Runtime.Serialization;

namespace GinRummy.Contracts.DataContracts.Friendship
{
    [DataContract]
    public class FriendshipEvent
    {
        [DataMember]
        public FriendshipEventType EventType { get; set; }

        [DataMember]
        public int OtherPlayerPublicTag { get; set; }

        [DataMember]
        public int FriendRequestId { get; set; }

        [DataMember]
        public string OtherPlayerUsername { get; set; }

        [DataMember]
        public DateTime SentAt { get; set; }
    }
}
