using System.Runtime.Serialization;

namespace GinRummy.Contracts.DataContracts.SocialCommunication
{
    [DataContract]
    public class MessageToSend
    {
        [DataMember]
        public MessageDestination Destination { get; set; }

        [DataMember]
        public string Content { get; set; }

        [DataMember]
        public int FriendPublicTag { get; set; }
    }
}
