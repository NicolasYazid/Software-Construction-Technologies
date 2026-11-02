using System;
using System.Runtime.Serialization;

namespace GinRummy.Contracts.Faults
{
    /// <summary>
    /// Detail of a sign-in refused because a ban restricts the whole account. The client shows the suspended
    /// account screen (P09) for a temporary ban and the banned account screen (P10) for a permanent one.
    /// </summary>
    [DataContract]
    public class SanctionFault
    {
        /// <summary>
        /// Name of the reason of the ban, in the language of the player.
        /// </summary>
        [DataMember]
        public string ReasonName { get; set; }

        /// <summary>
        /// Moment the ban ends, in UTC; null when the ban is permanent.
        /// </summary>
        [DataMember]
        public DateTime? ExpiresAt { get; set; }
    }
}
