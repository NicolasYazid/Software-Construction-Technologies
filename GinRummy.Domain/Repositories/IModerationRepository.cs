using System.Collections.Generic;

using GinRummy.Domain.Entities;

namespace GinRummy.Domain.Repositories
{
    /// <summary>
    /// Contract the game's logic uses to reach reports, warnings and bans together with their
    /// catalogs, without knowing how or where they are stored.
    /// </summary>
    public interface IModerationRepository
    {
        /// <summary>
        /// Gets the report categories a player can choose from, each with its translations.
        /// </summary>
        /// <returns>The categories.</returns>
        IList<ReportCategory> GetReportCategories();

        /// <summary>
        /// Finds a report status by its name, or null when the catalog has no such status.
        /// </summary>
        /// <param name="name">Name of the status, such as ReportStatus.PendingName.</param>
        /// <returns>The matching status, or null.</returns>
        ReportStatus FindReportStatusByName(string name);

        /// <summary>
        /// Finds a sanction reason by its identifier, or null when it does not exist.
        /// </summary>
        /// <param name="reasonBanId">Identifier of the reason.</param>
        /// <returns>The matching reason, or null.</returns>
        ReasonBan FindReasonBanById(int reasonBanId);

        /// <summary>
        /// Adds a new report and persists it immediately, so its identifier is known right
        /// after the call and a warning can reference it.
        /// </summary>
        /// <param name="newReport">Report to create.</param>
        void AddReport(Report newReport);

        /// <summary>
        /// Moves a report to another status and persists the change.
        /// </summary>
        /// <param name="reportId">Report to update.</param>
        /// <param name="reportStatusId">Status the report moves to.</param>
        void ChangeReportStatus(int reportId, int reportStatusId);

        /// <summary>
        /// Adds a new warning and persists it immediately.
        /// </summary>
        /// <param name="newWarning">Warning to create.</param>
        void AddWarning(Warning newWarning);

        /// <summary>
        /// Adds a new ban and persists it immediately, so its identifier is known right
        /// after the call.
        /// </summary>
        /// <param name="newBan">Ban to create.</param>
        void AddBan(Ban newBan);

        /// <summary>
        /// Links a warning to the ban it ended up causing and persists the change.
        /// </summary>
        /// <param name="warningId">Warning to update.</param>
        /// <param name="banId">Ban the warning led to.</param>
        void AssociateWarningWithBan(int warningId, int banId);

        /// <summary>
        /// Gets every ban a player has received, active or not, each with its reason loaded.
        /// </summary>
        /// <param name="playerId">Player whose bans are wanted.</param>
        /// <returns>The bans, most recent first.</returns>
        IList<Ban> GetBansOf(int playerId);

        /// <summary>
        /// Gets every warning a player has received.
        /// </summary>
        /// <param name="playerId">Player whose warnings are wanted.</param>
        /// <returns>The warnings, most recent first.</returns>
        IList<Warning> GetWarningsOf(int playerId);
    }
}
