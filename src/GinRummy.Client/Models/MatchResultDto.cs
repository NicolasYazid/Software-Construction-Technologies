namespace GinRummy.Client.Models
{
    public sealed class MatchResultDto
    {
        public MatchEndReason EndReason { get; set; }
        public int PlayerScore { get; set; }
        public int OpponentScore { get; set; }
    }
}
