using System.Runtime.Serialization;

namespace GinRummy.Contracts.DataContracts
{
    /// <summary>
    /// Link of a player to one social network.
    /// </summary>
    [DataContract]
    public class SocialLinkDetails
    {
        /// <summary>
        /// Name of the social network.
        /// </summary>
        [DataMember]
        public string PlatformName { get; set; }

        /// <summary>
        /// Address of the player in that social network.
        /// </summary>
        [DataMember]
        public string Url { get; set; }
    }
}
