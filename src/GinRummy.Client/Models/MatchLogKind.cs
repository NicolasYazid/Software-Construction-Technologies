namespace GinRummy.Client.Models
{
    // Each member needs its own sentence in the dictionary.
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
