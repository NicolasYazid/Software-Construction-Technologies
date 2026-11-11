using GinRummy.Domain;

namespace GinRummy.Application.Ports.Gameplay
{
    public interface IPlayRoundUseCase
    {
        void TakeFirstCard(int playerId);

        void PassFirstCard(int playerId);

        void DrawFromStock(int playerId);

        void DrawFromDiscardPile(int playerId);

        void Discard(int playerId, Card card);

        void Knock(int playerId, Card discardedCard);

        void ForfeitRound(int playerId);
    }
}
