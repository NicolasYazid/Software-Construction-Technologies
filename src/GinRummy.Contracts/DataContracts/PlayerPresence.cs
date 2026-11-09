using System.Runtime.Serialization;

namespace GinRummy.Contracts.DataContracts
{
    [DataContract]
    public class PlayerPresence
    {
        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public int? PublicTag { get; set; }

        [DataMember]
        public PresenceStatus Status { get; set; }

        [DataMember]
        public bool IsFriend { get; set; }

        [DataMember]
        public string RankName { get; set; }

        [DataMember]
        public int Score { get; set; }

        [DataMember]
        public bool IsGuest { get; set; }

        [DataMember]
        public int? GuestSessionId { get; set; }
    }
}
