using GinRummy.Application.UseCases.Gameplay;

namespace GinRummy.Application.Ports.Gameplay
{
    public interface IRoundAnnouncer
    {
        void AnnounceRoundStarted(int playerId, ActiveMatch match);

        void AnnounceRoundChanged(int playerId, ActiveMatch match);

        void AnnounceRoundFinished(int playerId, ActiveMatch match);

        void AnnounceOpponentLeft(int playerId);
    }
}
