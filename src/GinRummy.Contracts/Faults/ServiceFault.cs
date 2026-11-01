using System.Runtime.Serialization;

namespace GinRummy.Contracts.Faults
{
    /// <summary>
    /// Detail of a controlled failure of an operation. It only carries the code the client needs to choose
    /// the message, never internal details of the server.
    /// </summary>
    [DataContract]
    public class ServiceFault
    {
        /// <summary>
        /// Cause of the failure.
        /// </summary>
        [DataMember]
        public ServiceFaultCode Code { get; set; }
    }
}
