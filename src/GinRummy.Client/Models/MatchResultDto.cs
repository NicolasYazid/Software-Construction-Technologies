namespace GinRummy.Client.Models
{
    // Result of a finished match, as the table announces it.
    public sealed class MatchResultDto
    {
        public MatchEndReason EndReason { get; set; }
        public int PlayerScore { get; set; }
        public int OpponentScore { get; set; }
    }
}
