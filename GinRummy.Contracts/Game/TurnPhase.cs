using System.Runtime.Serialization;

namespace GinRummy.Contracts.Game
{
    /// <summary>
    /// Step of the turn the player whose turn it is must complete next.
    /// </summary>
    [DataContract]
    public enum TurnPhase
    {
        /// <summary>
        /// Take or pass the card turned up after the deal (house rule 2).
        /// </summary>
        [EnumMember]
        UpcardDecision,

        /// <summary>
        /// Draw one card, from the stock or from the discard pile (house rule 3).
        /// </summary>
        [EnumMember]
        Draw,

        /// <summary>
        /// Discard one card, or close the hand with that discard (house rules 3 and 8).
        /// </summary>
        [EnumMember]
        Discard,

        /// <summary>
        /// The hand ended; both players must confirm before the next deal.
        /// </summary>
        [EnumMember]
        HandEnded
    }
}