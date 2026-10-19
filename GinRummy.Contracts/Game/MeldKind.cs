using System.Runtime.Serialization;

namespace GinRummy.Contracts.Game
{
    /// <summary>
    /// Kind of meld, as house rule 5 defines them.
    /// </summary>
    [DataContract]
    public enum MeldKind
    {
        /// <summary>
        /// Three or four cards of the same rank.
        /// </summary>
        [EnumMember]
        Set,

        /// <summary>
        /// Three or more consecutive cards of the same suit.
        /// </summary>
        [EnumMember]
        Run
    }
}