namespace GinRummy.Client.Models
{
    /// <summary>
    /// Kind of event the match log records. Each one has its own sentence in the dictionary.
    /// </summary>
    public enum MatchLogKind
    {
        /// <summary>
        /// A player dealt the cards; the entry carries how many.
        /// </summary>
        Dealt,

        /// <summary>
        /// A player turned up the card that starts the discard pile; the entry carries it.
        /// </summary>
        TurnedUp,

        /// <summary>
        /// A player passed on the card of the discard pile.
        /// </summary>
        Passed,

        /// <summary>
        /// A player drew from the stock.
        /// </summary>
        DrewFromStock,

        /// <summary>
        /// A player discarded a card; the entry carries it.
        /// </summary>
        Discarded,

        /// <summary>
        /// A player knocked; the entry carries the deadwood.
        /// </summary>
        Knocked,

        /// <summary>
        /// A player knocked with no unmatched cards.
        /// </summary>
        Gin,

        /// <summary>
        /// The table waits for the move of the player.
        /// </summary>
        Waiting
    }
}
