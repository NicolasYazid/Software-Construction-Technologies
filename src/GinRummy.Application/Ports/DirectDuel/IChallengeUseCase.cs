namespace GinRummy.Application.Ports.DirectDuel
{
    public interface IChallengeUseCase
    {
        void Challenge(int challengerId, int challengedPublicTag);

        void Cancel(int challengerId);

        void Accept(int challengedId, int challengerPublicTag);

        void Decline(int challengedId, int challengerPublicTag);
    }
}
