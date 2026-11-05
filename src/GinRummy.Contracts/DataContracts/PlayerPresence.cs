using System.Runtime.Serialization;

namespace GinRummy.Contracts.DataContracts
{
    [DataContract]
    public class PlayerPresence
    {
        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public int PublicTag { get; set; }

        [DataMember]
        public PresenceStatus Status { get; set; }

        [DataMember]
        public bool IsFriend { get; set; }

        /// <summary>
        /// Gets or sets the name of the rank that the score of the player falls in.
        /// </summary>
        [DataMember]
        public string RankName { get; set; }

        /// <summary>
        /// Gets or sets the score of the player. The client orders its lists by it, because a higher score always
        /// falls in a higher rank.
        /// </summary>
        [DataMember]
        public int Score { get; set; }

        /// <summary>
        /// Gets or sets whether the player is a guest. A guest has no profile or friends, so the client offers it
        /// only what CU-27 FA-04 allows.
        /// </summary>
        [DataMember]
        public bool IsGuest { get; set; }
    }
}
