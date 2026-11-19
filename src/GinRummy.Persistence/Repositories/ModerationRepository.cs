using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using GinRummy.Domain.Entities.PlayerModeration;
using GinRummy.Domain.Ports.PlayerModeration;
using GinRummy.Persistence.Persistence;

namespace GinRummy.Persistence.Repositories
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
        /// Adds a new report and persists it immediately, so its identifier is known right
        /// after the call.
        /// </summary>
        /// <param name="newReport">Report to create.</param>
        public void AddReport(Report newReport)
        {
            _context.Reports.Add(newReport);
            _context.SaveChanges();
        }

        /// <summary>
        /// Adds a new warning and persists it immediately, so its identifier is known right
        /// after the call.
        /// </summary>
        /// <param name="newWarning">Warning to create.</param>
        public void AddWarning(Warning newWarning)
        {
            _context.Warnings.Add(newWarning);
            _context.SaveChanges();
        }

        /// <summary>
        /// Adds a new sanction and persists it immediately, so its identifier is known right
        /// after the call.
        /// </summary>
        /// <param name="newSanction">Sanction to create.</param>
        public void AddSanction(Sanction newSanction)
        {
            _context.Sanctions.Add(newSanction);
            _context.SaveChanges();
        }

        /// <summary>
        /// Adds a new permanent ban and persists it immediately.
        /// </summary>
        /// <param name="newPermanentBan">Permanent ban to create.</param>
        public void AddPermanentBan(PermanentBan newPermanentBan)
        {
            _context.PermanentBans.Add(newPermanentBan);
            _context.SaveChanges();
        }

        /// <summary>
        /// Counts every warning a player has received.
        /// </summary>
        /// <param name="playerAccountId">Player whose warnings are counted.</param>
        /// <returns>The number of warnings.</returns>
        public int CountWarningsOf(int playerAccountId)
        {
            return _context.Warnings.Count(warning => warning.PlayerAccountId == playerAccountId);
        }

        /// <summary>
        /// Counts every sanction a player has received.
        /// </summary>
        /// <param name="playerAccountId">Player whose sanctions are counted.</param>
        /// <returns>The number of sanctions.</returns>
        public int CountSanctionsOf(int playerAccountId)
        {
            return _context.Sanctions.Count(sanction => sanction.PlayerAccountId == playerAccountId);
        }

        /// <summary>
        /// Gets the sanctions of a player that have not ended at the given moment: the one in
        /// force and the ones waiting for it.
        /// </summary>
        /// <param name="playerAccountId">Player whose sanctions are wanted.</param>
        /// <param name="moment">Moment the sanctions are compared with.</param>
        /// <returns>The sanctions, in the order they start.</returns>
        public IList<Sanction> GetUnfinishedSanctionsOf(int playerAccountId, DateTime moment)
        {
            return _context.Sanctions
                .Where(sanction => (sanction.PlayerAccountId == playerAccountId) && (sanction.EndsAt > moment))
                .OrderBy(sanction => sanction.StartsAt)
                .ToList();
        }

        /// <summary>
        /// Gets every warning a player has received, each with the report that caused it.
        /// </summary>
        /// <param name="playerAccountId">Player whose warnings are wanted.</param>
        /// <returns>The warnings, most recent first.</returns>
        public IList<Warning> GetWarningsOf(int playerAccountId)
        {
            return _context.Warnings
                .Include(warning => warning.Report)
                .Where(warning => warning.PlayerAccountId == playerAccountId)
                .OrderByDescending(warning => warning.IssuedAt)
                .ToList();
        }

        /// <summary>
        /// Tells whether a player has a permanent ban.
        /// </summary>
        /// <param name="playerAccountId">Player to check.</param>
        /// <returns>True when the account is permanently banned.</returns>
        public bool IsPermanentlyBanned(int playerAccountId)
        {
            return _context.PermanentBans.Any(permanentBan => permanentBan.PlayerAccountId == playerAccountId);
        }

        /// <summary>
        /// Gets the identifier of every permanently banned account.
        /// </summary>
        /// <returns>The identifiers of the banned accounts.</returns>
        public IList<int> GetPermanentlyBannedPlayerIds()
        {
            return _context.PermanentBans
                .Select(permanentBan => permanentBan.PlayerAccountId)
                .ToList();
        }

        /// <summary>
        /// Gets the prohibited words with the locales each one applies to.
        /// </summary>
        /// <returns>The prohibited words.</returns>
        public IList<ProhibitedWord> GetProhibitedWords()
        {
            return _context.ProhibitedWords
                .Include(word => word.Locales)
                .ToList();
        }
    }
}
