using GinRummy.Application.UseCases.DirectDuel;
using GinRummy.Application.UseCases.Gameplay;

namespace GinRummy.Application.Ports.DirectDuel
{
    public interface IChallengeAnnouncer
    {
        bool IsPlayerOnline(int playerId);

        void AnnounceChallengeReceived(PendingChallenge challenge);

        void AnnounceChallengeCancelled(PendingChallenge challenge);

        void AnnounceChallengeDeclined(PendingChallenge challenge);

        void AnnounceRoundStarted(ActiveRound round);
    }
}
