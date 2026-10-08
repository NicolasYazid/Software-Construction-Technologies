namespace GinRummy.Client.Models
{
    // Reason a match ended, seen from the player who looks at the table.
    // It decides both the verdict and the sentence that explains it.
    public enum MatchEndReason
    {
        PlayerReachedTarget,
        // The opponent forfeited the match: a victory (CU-26).
        OpponentForfeited,
        OpponentReachedTarget
    }
}
