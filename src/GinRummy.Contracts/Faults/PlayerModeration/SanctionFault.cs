using System;
using System.Runtime.Serialization;

namespace GinRummy.Contracts.Faults.PlayerModeration
{
    [DataContract]
    public class SanctionFault
    {
        [DataMember]
        public string ReasonName { get; set; }

        [DataMember]
        public DateTime? ExpiresAt { get; set; }
    }
}
