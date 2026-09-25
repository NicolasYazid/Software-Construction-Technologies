using System;

namespace GinRummy.Client.Models
{
    /// <summary>
    /// Entry of the match log. It carries the event and its data, not a sentence, so that the
    /// table can write it in the active language.
    /// </summary>
    public sealed class MatchLogEntryDto
    {
        /// <summary>
        /// Gets or sets the name of the player the event belongs to.
        /// </summary>
        public string PlayerName { get; set; }
        /// <summary>
        /// Gets or sets when the event happened.
        /// </summary>
        public DateTime OccurredAt { get; set; }
        /// <summary>
        /// Gets or sets the kind of event.
        /// </summary>
        public MatchLogKind Kind { get; set; }
        /// <summary>
        /// Gets or sets the card of the event, for the kinds that name one.
        /// </summary>
        public CardDto Card { get; set; }
        /// <summary>
        /// Gets or sets the number of the event, such as the cards dealt or the deadwood of a
        /// knock.
        /// </summary>
        public int Amount { get; set; }
    }
}
