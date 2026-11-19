using System.Runtime.Serialization;

namespace GinRummy.Contracts.Faults.PlayerModeration
{
    [DataContract]
    public class AccountBannedFault
    {
        [DataMember]
        public string ReasonKey { get; set; }

        [DataMember]
        public string MessageKey { get; set; }
    }
}
