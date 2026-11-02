using System.Runtime.Serialization;

namespace GinRummy.Contracts.DataContracts
{
    [DataContract]
    public class PlayerPresence
    {
        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public int PublicTag { get; set; }

        [DataMember]
        public PresenceStatus Status { get; set; }

        [DataMember]
        public bool IsFriend { get; set; }
    }
}
