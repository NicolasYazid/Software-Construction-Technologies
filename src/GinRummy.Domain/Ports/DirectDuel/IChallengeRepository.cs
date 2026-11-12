using System.Collections.Generic;

using GinRummy.Domain.Entities.DirectDuel;

namespace GinRummy.Domain.Ports.DirectDuel
{
    public interface IChallengeRepository
    {
        Challenge FindById(int challengeId);

        IList<Challenge> GetPendingReceivedBy(int challengedPlayerAccountId);

        IList<Challenge> GetPendingSentBy(int challengerPlayerAccountId);

        void Add(Challenge newChallenge);
    }
}
