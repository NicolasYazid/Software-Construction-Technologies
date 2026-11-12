using System.Runtime.Serialization;

namespace GinRummy.Contracts.Faults.AccountProtection
{
    [DataContract]
    public class ResendLimitFault
    {
        [DataMember]
        public int RetryAfterSeconds { get; set; }
    }
}
