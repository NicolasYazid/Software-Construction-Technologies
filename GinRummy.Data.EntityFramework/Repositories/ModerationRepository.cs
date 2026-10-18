using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using GinRummy.Data.EntityFramework.Persistence;
using GinRummy.Domain.Entities;
using GinRummy.Domain.Repositories;

namespace GinRummy.Data.EntityFramework.Repositories
{
    /// <summary>
    /// Fulfils IModerationRepository using Entity Framework against GinRummy_Dev. It always runs
    /// on the context of the unit of work that created it, so its changes share that unit's
    /// transaction.
    /// </summary>
    public class ModerationRepository : IModerationRepository
    {
        private readonly GinRummyContext _context;

        /// <summary>
        /// Builds the repository on a context someone else opened. The caller stays responsible
        /// for disposing that context.
        /// </summary>
        /// <param name="context">Context to run every operation on.</param>
        public ModerationRepository(GinRummyContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Finds a sanction reason by its identifier, or null when it does not exist.
        /// </summary>
        /// <param name="reasonBanId">Identifier of the reason.</param>
        /// <returns>The matching reason, or null.</returns>
        public ReasonBan FindReasonBanById(int reasonBanId)
        {
            return _context.ReasonBans
                .Include(reason => reason.Translations)
                .FirstOrDefault(reason => reason.ReasonBanId == reasonBanId);
        }

        /// <summary>
        /// Adds a new warning and persists it immediately.
        /// </summary>
        /// <param name="newWarning">Warning to create.</param>
        public void AddWarning(Warning newWarning)
        {
            _context.Warnings.Add(newWarning);
            _context.SaveChanges();
        }

        /// <summary>
        /// Adds a new ban and persists it immediately, so its identifier is known right
        /// after the call.
        /// </summary>
        /// <param name="newBan">Ban to create.</param>
        public void AddBan(Ban newBan)
        {
            _context.Bans.Add(newBan);
            _context.SaveChanges();
        }

        /// <summary>
        /// Links a warning to the ban it ended up causing and persists the change.
        /// </summary>
        /// <param name="warningId">Warning to update.</param>
        /// <param name="banId">Ban the warning led to.</param>
        /// <exception cref="InvalidOperationException">Thrown when the warning does not exist.</exception>
        public void AssociateWarningWithBan(int warningId, int banId)
        {
            Warning warning = _context.Warnings.FirstOrDefault(found => found.WarningId == warningId);
            if (warning == null)
            {
                throw new InvalidOperationException("The warning does not exist.");
            }

            warning.AssociateWithBan(banId);
            _context.SaveChanges();
        }

        /// <summary>
        /// Gets every ban a player has received, active or not, each with its reason and the
        /// translations of the reason loaded.
        /// </summary>
        /// <param name="playerId">Player whose bans are wanted.</param>
        /// <returns>The bans, most recent first.</returns>
        public IList<Ban> GetBansOf(int playerId)
        {
            return _context.Bans
                .Include(ban => ban.ReasonBan.Translations)
                .Where(ban => ban.PlayerId == playerId)
                .OrderByDescending(ban => ban.IssuedAt)
                .ToList();
        }

        /// <summary>
        /// Gets every warning a player has received.
        /// </summary>
        /// <param name="playerId">Player whose warnings are wanted.</param>
        /// <returns>The warnings, most recent first.</returns>
        public IList<Warning> GetWarningsOf(int playerId)
        {
            return _context.Warnings
                .Where(warning => warning.PlayerId == playerId)
                .OrderByDescending(warning => warning.IssuedAt)
                .ToList();
        }
    }
}
