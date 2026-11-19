using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using GinRummy.Domain.Entities.DirectDuel;
using GinRummy.Domain.Ports.DirectDuel;
using GinRummy.Persistence.Persistence;

namespace GinRummy.Persistence.Repositories
{
    public class ChallengeRepository : IChallengeRepository
    {
        private readonly GinRummyContext _context;

        public ChallengeRepository(GinRummyContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Challenge FindById(int challengeId)
        {
            return _context.Challenges.FirstOrDefault(challenge => challenge.ChallengeId == challengeId);
        }

        public IList<Challenge> GetPendingReceivedBy(int challengedPlayerAccountId)
        {
            string sent = ChallengeStatus.Sent.ToString();

            return _context.Challenges
                .Include(challenge => challenge.Challenger.PlayerProfile)
                .Where(challenge => (challenge.ChallengedPlayerAccountId == challengedPlayerAccountId) && (challenge.StatusName == sent))
                .OrderBy(challenge => challenge.CreatedAt)
                .ToList();
        }

        public IList<Challenge> GetPendingSentBy(int challengerPlayerAccountId)
        {
            string sent = ChallengeStatus.Sent.ToString();

            return _context.Challenges
                .Include(challenge => challenge.Challenged.PlayerProfile)
                .Where(challenge => (challenge.ChallengerPlayerAccountId == challengerPlayerAccountId) && (challenge.StatusName == sent))
                .OrderBy(challenge => challenge.CreatedAt)
                .ToList();
        }

        public void Add(Challenge newChallenge)
        {
            _context.Challenges.Add(newChallenge);
            _context.SaveChanges();
        }
    }
}
