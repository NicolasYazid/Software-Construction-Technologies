using System.Runtime.Serialization;

namespace GinRummy.Contracts.Game
{
    /// <summary>
    /// Move the opponent made, used by the match log of the table screen.
    /// </summary>
    [DataContract]
    public enum OpponentMoveKind
    {
        /// <summary>
        /// The opponent took the card turned up after the deal.
        /// </summary>
        [EnumMember]
        TookUpcard,

        /// <summary>
        /// The opponent passed on the card turned up after the deal.
        /// </summary>
        [EnumMember]
        PassedUpcard,

        /// <summary>
        /// The opponent drew the top card of the stock; the card stays hidden.
        /// </summary>
        [EnumMember]
        DrewFromStock,

        /// <summary>
        /// The opponent drew the top card of the discard pile; the card is visible.
        /// </summary>
        [EnumMember]
        DrewFromDiscardPile,

        /// <summary>
        /// The opponent discarded a card.
        /// </summary>
        [EnumMember]
        Discarded
    }
}