namespace GinRummy.Application.UseCases.DirectDuel
{
    public enum ChallengeOutcome
    {
        Done,
        PlayerNotAvailable,
        PlayerNotFound,
        AlreadyChallenging,
        OpponentBusy,
        AccountSuspended,
        ChallengeExpired
    }
}