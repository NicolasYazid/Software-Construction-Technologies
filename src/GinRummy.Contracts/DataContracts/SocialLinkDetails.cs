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
        /// Identifier of the link, which the owner uses to remove it.
        /// </summary>
        [DataMember]
        public int SocialMediaId { get; set; }

        /// <summary>
        /// Name of the social network, as the player wrote it.
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
