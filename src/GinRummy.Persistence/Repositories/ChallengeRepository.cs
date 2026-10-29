using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using GinRummy.Domain.Entities;
using GinRummy.Domain.Repositories;
using GinRummy.Persistence.Persistence;

namespace GinRummy.Persistence.Repositories
{
    /// <summary>
    /// Fulfils IChallengeRepository using Entity Framework against GinRummy_Dev. It always
    /// runs on the context of the unit of work that created it, so its changes share that
    /// unit's transaction.
    /// </summary>
    public class ChallengeRepository : IChallengeRepository
    {
        private readonly GinRummyContext _context;

        /// <summary>
        /// Builds the repository on a context someone else opened. The caller stays responsible
        /// for disposing that context.
        /// </summary>
        /// <param name="context">Context to run every operation on.</param>
        public ChallengeRepository(GinRummyContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Finds a challenge by its identifier, or null when it does not exist.
        /// </summary>
        /// <param name="challengeId">Identifier of the challenge.</param>
        /// <returns>The matching challenge, or null.</returns>
        public Challenge FindById(int challengeId)
        {
            return _context.Challenges.FirstOrDefault(challenge => challenge.ChallengeId == challengeId);
        }

        /// <summary>
        /// Gets the challenges a player received that still wait for an answer, oldest first.
        /// </summary>
        /// <param name="challengedPlayerId">Player who received the challenges.</param>
        /// <returns>The pending challenges; empty when there are none.</returns>
        public IList<Challenge> GetPendingReceivedBy(int challengedPlayerId)
        {
            // The status is turned into text before the query because EF cannot translate the
            // computed Status property to SQL.
            string sent = ChallengeStatus.Sent.ToString();

            // The challenger and its player are loaded together because the caller needs the
            // username of each challenger to show the list.
            return _context.Challenges
                .Include(challenge => challenge.Challenger.Player)
                .Where(challenge => (challenge.ChallengedPlayerId == challengedPlayerId) && (challenge.StatusName == sent))
                .OrderBy(challenge => challenge.CreatedAt)
                .ToList();
        }

        /// <summary>
        /// Gets the challenges a player sent that still wait for an answer, oldest first.
        /// </summary>
        /// <param name="challengerPlayerId">Player who sent the challenges.</param>
        /// <returns>The pending challenges; empty when there are none.</returns>
        public IList<Challenge> GetPendingSentBy(int challengerPlayerId)
        {
            string sent = ChallengeStatus.Sent.ToString();

            return _context.Challenges
                .Include(challenge => challenge.Challenged.Player)
                .Where(challenge => (challenge.ChallengerPlayerId == challengerPlayerId) && (challenge.StatusName == sent))
                .OrderBy(challenge => challenge.CreatedAt)
                .ToList();
        }

        /// <summary>
        /// Adds a new challenge and persists it immediately.
        /// </summary>
        /// <param name="newChallenge">Challenge to create.</param>
        public void Add(Challenge newChallenge)
        {
            _context.Challenges.Add(newChallenge);
            _context.SaveChanges();
        }
    }
}
