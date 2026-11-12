using System.Collections.Generic;

using GinRummy.Domain.Entities.Gameplay;

namespace GinRummy.Domain.Ports.Gameplay
{
    public interface IGameRepository
    {
        GameSession FindById(int gameSessionId);

        GameSession FindGameSessionInProgressByPlayerAccountId(int playerAccountId);

        GameSession FindOpenByGuestSessionId(int guestSessionId);

        void Add(GameSession newGameSession);

        void AddParticipant(GameParticipant newParticipant);

        void AddHistory(GameHistory newEntry);

        IList<GameHistory> GetHistoryOf(int playerAccountId);
    }
}
