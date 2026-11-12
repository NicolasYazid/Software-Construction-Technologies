using System.Runtime.Serialization;

namespace GinRummy.Contracts.Faults.Shared
{
    [DataContract]
    public class ServiceFault
    {
        [DataMember]
        public ServiceFaultCode Code { get; set; }
    }
}
