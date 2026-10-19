using System.Runtime.Serialization;

namespace GinRummy.Contracts.Game
{
    /// <summary>
    /// How a hand ended, which decides how its points are counted.
    /// </summary>
    [DataContract]
    public enum HandOutcome
    {
        /// <summary>
        /// The player who closed had less deadwood and wins the difference (house rule 9).
        /// </summary>
        [EnumMember]
        Knock,

        /// <summary>
        /// The player who closed had no deadwood and earns the gin bonus (house rule 10).
        /// </summary>
        [EnumMember]
        Gin,

        /// <summary>
        /// The defender had equal or less deadwood and earns the undercut bonus (house rule 11).
        /// </summary>
        [EnumMember]
        Undercut,

        /// <summary>
        /// The stock reached the void limit and nobody scores (house rule 12).
        /// </summary>
        [EnumMember]
        Void
    }
}