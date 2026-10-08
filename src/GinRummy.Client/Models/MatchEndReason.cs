namespace GinRummy.Client.Models
{
    // The reason is relative to the player who looks at the table, so each player of the same match receives a different one.
    public enum MatchEndReason
    {
        PlayerReachedTarget,
        // A forfeit of the opponent counts as a victory for the player (CU-26).
        OpponentForfeited,
        OpponentReachedTarget
    }
}
