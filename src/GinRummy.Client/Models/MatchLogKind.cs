namespace GinRummy.Client.Models
{
    // Kind of event the match log records. Each one has its own sentence in the dictionary.
    public enum MatchLogKind
    {
        Dealt,
        TurnedUp,
        Passed,
        DrewFromStock,
        Discarded,
        Knocked,
        Gin,
        Waiting
    }
}
