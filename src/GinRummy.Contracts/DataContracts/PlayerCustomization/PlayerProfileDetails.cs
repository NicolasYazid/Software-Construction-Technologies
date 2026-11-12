using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GinRummy.Contracts.DataContracts.PlayerCustomization
{
    [DataContract]
    public class PlayerProfileDetails
    {
        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public int PublicTag { get; set; }

        [DataMember]
        public string Bio { get; set; }

        [DataMember]
        public List<SocialLinkDetails> SocialLinks { get; set; }

        [DataMember]
        public int MatchesPlayed { get; set; }

        [DataMember]
        public double WinRate { get; set; }

        [DataMember]
        public int Score { get; set; }

        [DataMember]
        public string RankName { get; set; }
    }
}
