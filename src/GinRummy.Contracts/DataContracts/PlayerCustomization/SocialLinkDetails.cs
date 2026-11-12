using System.Runtime.Serialization;

namespace GinRummy.Contracts.DataContracts.PlayerCustomization
{
    [DataContract]
    public class SocialLinkDetails
    {
        [DataMember]
        public int SocialMediaId { get; set; }

        [DataMember]
        public string PlatformName { get; set; }

        [DataMember]
        public string Url { get; set; }
    }
}
