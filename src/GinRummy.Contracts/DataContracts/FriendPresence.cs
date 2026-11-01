using System.Runtime.Serialization;

namespace GinRummy.Contracts.DataContracts
{
    /// <summary>
    /// Presence of one friend as the friends list shows it: who they are and whether they are online.
    /// </summary>
    [DataContract]
    public class FriendPresence
    {
        /// <summary>
        /// Gets or sets the public tag of the friend.
        /// </summary>
        [DataMember]
        public int PublicTag { get; set; }

        /// <summary>
        /// Gets or sets the display name of the friend.
        /// </summary>
        [DataMember]
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the presence state of the friend.
        /// </summary>
        [DataMember]
        public PresenceStatus Status { get; set; }
    }
}
