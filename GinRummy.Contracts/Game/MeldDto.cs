using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GinRummy.Contracts.Game
{
    /// <summary>
    /// A meld laid down when a hand closes. The server builds it; the client only shows it.
    /// </summary>
    [DataContract]
    public class MeldDto
    {
        /// <summary>
        /// Gets or sets whether the meld is a set or a run.
        /// </summary>
        [DataMember]
        public MeldKind Kind { get; set; }

        /// <summary>
        /// Gets or sets the cards of the meld; a run lists them in ascending order.
        /// </summary>
        [DataMember]
        public List<CardDto> Cards { get; set; }
    }
}