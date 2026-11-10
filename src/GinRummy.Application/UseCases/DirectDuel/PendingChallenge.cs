namespace GinRummy.Application.UseCases.DirectDuel
{
    public class PendingChallenge
    {
        public PendingChallenge(DuelPlayer challenger, DuelPlayer challenged)
        {
            Challenger = challenger;
            Challenged = challenged;
        }

        public DuelPlayer Challenger { get; private set; }

        public DuelPlayer Challenged { get; private set; }
    }
}
