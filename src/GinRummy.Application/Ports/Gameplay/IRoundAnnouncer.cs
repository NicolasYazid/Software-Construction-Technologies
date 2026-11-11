using GinRummy.Application.UseCases.Gameplay;

namespace GinRummy.Application.Ports.Gameplay
{
    public interface IRoundAnnouncer
    {
        void AnnounceRoundStarted(int playerId, ActiveRound round);

        void AnnounceRoundChanged(int playerId, ActiveRound round);

        void AnnounceRoundFinished(int playerId, ActiveRound round);

        void AnnounceOpponentLeft(int playerId);
    }
}
