using System.Runtime.Serialization;

namespace GinRummy.Contracts.Game
{
    /// <summary>
    /// Reason the server rejected a move. Moves are one-way and cannot return a fault, so the reason travels
    /// through the callback and the client translates it with its resource files.
    /// </summary>
    [DataContract]
    public enum MoveRejectionCode
    {
        /// <summary>
        /// It is the opponent's turn.
        /// </summary>
        [EnumMember]
        NotYourTurn,

        /// <summary>
        /// The move does not belong to the current step of the turn, such as discarding before drawing
        /// (house rule 16).
        /// </summary>
        [EnumMember]
        WrongPhase,

        /// <summary>
        /// The card is not in the player's hand.
        /// </summary>
        [EnumMember]
        CardNotInHand,

        /// <summary>
        /// The card is the one just taken from the discard pile (house rule 4).
        /// </summary>
        [EnumMember]
        CardJustTaken,

        /// <summary>
        /// The deadwood left after the discard is above the knock limit (house rule 8).
        /// </summary>
        [EnumMember]
        DeadwoodTooHigh,

        /// <summary>
        /// The match is paused while the opponent reconnects (DEC-09).
        /// </summary>
        [EnumMember]
        MatchPaused,

        /// <summary>
        /// The match already finished.
        /// </summary>
        [EnumMember]
        MatchFinished
    }
}