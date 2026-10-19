using System.Runtime.Serialization;

using GinRummy.Contracts.Common;

namespace GinRummy.Contracts.Game
{
    /// <summary>
    /// Request to sit at the table of a match that matchmaking (MatchFound) or an accepted challenge created.
    /// </summary>
    [DataContract]
    public class JoinTableRequest : SessionRequest
    {
        /// <summary>
        /// Gets or sets the match the MatchFound notification announced.
        /// </summary>
        [DataMember]
        public int GameSessionId { get; set; }
    }
}