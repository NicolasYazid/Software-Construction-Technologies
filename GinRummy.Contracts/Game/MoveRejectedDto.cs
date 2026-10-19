using System.Runtime.Serialization;

namespace GinRummy.Contracts.Game
{
    /// <summary>
    /// Answer to a move the server did not accept. The table stays as it was.
    /// </summary>
    [DataContract]
    public class MoveRejectedDto
    {
        /// <summary>
        /// Gets or sets why the move was rejected.
        /// </summary>
        [DataMember]
        public MoveRejectionCode Code { get; set; }
    }
}