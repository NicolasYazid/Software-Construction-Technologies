using System;
using System.Runtime.Serialization;

namespace GinRummy.Contracts.DataContracts
{
    [DataContract]
    public class PlayerMessage
    {
        [DataMember]
        public Guid MessageId { get; set; }

        [DataMember]
        public MessageDestination Destination { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public int PublicTag { get; set; }

        [DataMember]
        public string ProfilePicture { get; set; }

        [DataMember]
        public string Content { get; set; }

        [DataMember]
        public DateTime SentAt { get; set; }
    }
}
