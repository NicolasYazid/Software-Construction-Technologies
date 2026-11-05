namespace GinRummy.Client.Models
{
    // Each member needs its own sentence in the dictionary, because the table writes every event of the log in the active language.
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
