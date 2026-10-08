using System;

namespace GinRummy.Client.Models
{
    // It carries the event and its data, not a sentence, so that the table can write it in the active language.
    public sealed class MatchLogEntryDto
    {
        public string PlayerName { get; set; }
        public DateTime OccurredAt { get; set; }
        public MatchLogKind Kind { get; set; }
        public CardDto Card { get; set; }
        public int Amount { get; set; }
    }
}
