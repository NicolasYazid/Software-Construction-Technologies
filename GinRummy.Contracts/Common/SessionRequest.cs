using System.Runtime.Serialization;

namespace GinRummy.Contracts.Common
{
    /// <summary>
    /// Base of every request that needs an open session. The server checks the token on each call instead of
    /// trusting the client (DEC-05).
    /// </summary>
    [DataContract]
    public class SessionRequest
    {
        /// <summary>
        /// Gets or sets the opaque token that SignIn or SignInAsGuest returned.
        /// </summary>
        [DataMember]
        public string SessionToken { get; set; }
    }
}