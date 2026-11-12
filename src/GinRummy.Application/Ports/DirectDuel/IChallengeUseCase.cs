using GinRummy.Application.UseCases.DirectDuel;

namespace GinRummy.Application.Ports.DirectDuel
{
    public interface IChallengeUseCase
    {
        ChallengeOutcome Challenge(int challengerId, int challengedPublicTag);

        void Cancel(int challengerId);

        ChallengeOutcome Accept(int challengedId, int challengerPublicTag);

        ChallengeOutcome Decline(int challengedId, int challengerPublicTag);
    }
}