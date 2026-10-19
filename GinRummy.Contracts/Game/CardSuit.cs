using System.Runtime.Serialization;

namespace GinRummy.Contracts.Game
{
    /// <summary>
    /// Suit of a card.
    /// </summary>
    [DataContract]
    public enum CardSuit
    {
        /// <summary>
        /// Clubs.
        /// </summary>
        [EnumMember]
        Clubs,

        /// <summary>
        /// Diamonds.
        /// </summary>
        [EnumMember]
        Diamonds,

        /// <summary>
        /// Hearts.
        /// </summary>
        [EnumMember]
        Hearts,

        /// <summary>
        /// Spades.
        /// </summary>
        [EnumMember]
        Spades
    }
}