namespace GinRummy.Domain
{
    public sealed class RoundResult
    {
        public RoundResult(int? winnerId, int points, RoundOutcome outcome)
        {
            WinnerId = winnerId;
            Points = points;
            Outcome = outcome;
        }

        public int? WinnerId { get; private set; }
        public int Points { get; private set; }
        public RoundOutcome Outcome { get; private set; }
    }
}
