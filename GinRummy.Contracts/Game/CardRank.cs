using System.Runtime.Serialization;

namespace GinRummy.Contracts.Game
{
    /// <summary>
    /// Rank of a card. Each value is the number of the card, so the ace is always 1 (house rule 6) and the
    /// deadwood of a number card is its value (house rule 7).
    /// </summary>
    [DataContract]
    public enum CardRank
    {
        /// <summary>
        /// Ace, worth 1 and always at the start of a run.
        /// </summary>
        [EnumMember]
        Ace = 1,

        /// <summary>
        /// Two.
        /// </summary>
        [EnumMember]
        Two = 2,

        /// <summary>
        /// Three.
        /// </summary>
        [EnumMember]
        Three = 3,

        /// <summary>
        /// Four.
        /// </summary>
        [EnumMember]
        Four = 4,

        /// <summary>
        /// Five.
        /// </summary>
        [EnumMember]
        Five = 5,

        /// <summary>
        /// Six.
        /// </summary>
        [EnumMember]
        Six = 6,

        /// <summary>
        /// Seven.
        /// </summary>
        [EnumMember]
        Seven = 7,

        /// <summary>
        /// Eight.
        /// </summary>
        [EnumMember]
        Eight = 8,

        /// <summary>
        /// Nine.
        /// </summary>
        [EnumMember]
        Nine = 9,

        /// <summary>
        /// Ten, worth 10.
        /// </summary>
        [EnumMember]
        Ten = 10,

        /// <summary>
        /// Jack, worth 10.
        /// </summary>
        [EnumMember]
        Jack = 11,

        /// <summary>
        /// Queen, worth 10.
        /// </summary>
        [EnumMember]
        Queen = 12,

        /// <summary>
        /// King, worth 10 and always at the end of a run.
        /// </summary>
        [EnumMember]
        King = 13
    }
}