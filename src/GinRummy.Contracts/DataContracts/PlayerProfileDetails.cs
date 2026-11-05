using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GinRummy.Contracts.DataContracts
{
    /// <summary>
    /// Public information and performance of a registered player, as the profile screen shows them.
    /// </summary>
    [DataContract]
    public class PlayerProfileDetails
    {
        /// <summary>
        /// Name the player shows to the others.
        /// </summary>
        [DataMember]
        public string Username { get; set; }

        /// <summary>
        /// Tag that tells the player apart from others with the same name.
        /// </summary>
        [DataMember]
        public int PublicTag { get; set; }

        /// <summary>
        /// Text the player wrote to present itself, or null when it wrote none.
        /// </summary>
        [DataMember]
        public string Bio { get; set; }

        /// <summary>
        /// Links to the social networks of the player; empty when it linked none.
        /// </summary>
        [DataMember]
        public List<SocialLinkDetails> SocialLinks { get; set; }

        /// <summary>
        /// Number of matches the player finished.
        /// </summary>
        [DataMember]
        public int MatchesPlayed { get; set; }

        /// <summary>
        /// Share of the finished matches the player won, from zero to one.
        /// </summary>
        [DataMember]
        public double WinRate { get; set; }

        /// <summary>
        /// Score of the player.
        /// </summary>
        [DataMember]
        public int Score { get; set; }

        /// <summary>
        /// Name of the rank the score falls into, or empty when no rank contains it.
        /// </summary>
        [DataMember]
        public string RankName { get; set; }
    }
}
