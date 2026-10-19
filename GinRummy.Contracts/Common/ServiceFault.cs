using System.Runtime.Serialization;

namespace GinRummy.Contracts.Common
{
    /// <summary>
    /// Error that travels to the client. It carries only a code, which the client turns into a translated message
    /// with its resource files, so the server never sends text for the user.
    /// </summary>
    [DataContract]
    public class ServiceFault
    {
        /// <summary>
        /// Gets or sets the reason the operation did not complete.
        /// </summary>
        [DataMember]
        public ServiceFaultCode Code { get; set; }
    }
}