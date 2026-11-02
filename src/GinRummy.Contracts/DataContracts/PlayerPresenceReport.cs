using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GinRummy.Contracts.DataContracts
{
    [DataContract]
    public class PlayerPresenceReport
    {
        [DataMember]
        public List<PlayerPresence> AllOnlinePlayers { get; set; }

        [DataMember]
        public List<PlayerPresence> AllOfflinePlayers { get; set; }

        [DataMember]
        public List<PlayerPresence> AllOnlineFriends { get; set; }

        [DataMember]
        public List<PlayerPresence> AllOfflineFriends { get; set; }
    }
}
