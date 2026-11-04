namespace GinRummy.Client.Models
{
    public sealed class MatchResult
    {
        public MatchResult(MatchEndReason endReason, int playerScore, int opponentScore)
        {
            EndReason = endReason;
            PlayerScore = playerScore;
            OpponentScore = opponentScore;
        }

        public MatchEndReason EndReason { get; private set; }
        public int PlayerScore { get; private set; }
        public int OpponentScore { get; private set; }
    }
}
