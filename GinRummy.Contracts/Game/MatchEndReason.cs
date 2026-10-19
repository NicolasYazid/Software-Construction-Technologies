using System.Runtime.Serialization;

namespace GinRummy.Contracts.Game
{
    /// <summary>
    /// Why the match ended, seen from the player who receives it, so each player of the same match receives a
    /// different value.
    /// </summary>
    [DataContract]
    public enum MatchEndReason
    {
        /// <summary>
        /// The player reached the target score first (house rule 13).
        /// </summary>
        [EnumMember]
        PlayerReachedTarget,

        /// <summary>
        /// The opponent reached the target score first (house rule 13).
        /// </summary>
        [EnumMember]
        OpponentReachedTarget,

        /// <summary>
        /// The opponent forfeited or did not come back in time, which counts as a victory (house rule 19, CU-26).
        /// </summary>
        [EnumMember]
        OpponentForfeited
    }
}