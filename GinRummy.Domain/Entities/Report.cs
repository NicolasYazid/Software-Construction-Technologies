using System;

namespace GinRummy.Domain.Entities
{
    /// <summary>
    /// A complaint from one registered player against another, mapped to the Report table.
    /// A report is never deleted; only its status changes. Protects its own invariants: both
    /// players are real and they are not the same one.
    /// </summary>
    public class Report
    {
        /// <summary>
        /// Creates a valid new report.
        /// </summary>
        /// <param name="reporterPlayerId">Identifier of the player who reports; must be positive.</param>
        /// <param name="reportedPlayerId">Identifier of the reported player; must be positive and different.</param>
        /// <param name="reportCategoryId">Identifier of the reason picked; must be positive.</param>
        /// <param name="reportStatusId">Identifier of the starting status; must be positive.</param>
        /// <param name="createdAt">Moment the report was sent, in UTC.</param>
        public Report(int reporterPlayerId, int reportedPlayerId, int reportCategoryId, int reportStatusId, DateTime createdAt)
        {
            ValidatePlayers(reporterPlayerId, reportedPlayerId);
            ValidateId(reportCategoryId, nameof(reportCategoryId));
            ValidateId(reportStatusId, nameof(reportStatusId));

            ReporterPlayerId = reporterPlayerId;
            ReportedPlayerId = reportedPlayerId;
            ReportCategoryId = reportCategoryId;
            ReportStatusId = reportStatusId;
            CreatedAt = createdAt;
        }

        /// <summary>
        /// Parameterless constructor reserved for Entity Framework's materialization; not
        /// for application code, which must use the validating constructor.
        /// </summary>
        protected Report()
        {
        }

        /// <summary>
        /// Gets the internal, autonumeric identifier.
        /// </summary>
        public int ReportId { get; private set; }

        /// <summary>
        /// Gets the identifier of the player who sent the report.
        /// </summary>
        public int ReporterPlayerId { get; private set; }

        /// <summary>
        /// Gets the identifier of the player who was reported.
        /// </summary>
        public int ReportedPlayerId { get; private set; }

        /// <summary>
        /// Gets the identifier of the reason the reporter picked.
        /// </summary>
        public int ReportCategoryId { get; private set; }

        /// <summary>
        /// Gets the identifier of the current status of the report.
        /// </summary>
        public int ReportStatusId { get; private set; }

        /// <summary>
        /// Gets the moment the report was sent, in UTC.
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// Gets the reason the reporter picked, loaded from the relationship between the two
        /// tables.
        /// </summary>
        public virtual ReportCategory ReportCategory { get; private set; }

        /// <summary>
        /// Gets the current status of the report, loaded from the relationship between the
        /// two tables.
        /// </summary>
        public virtual ReportStatus ReportStatus { get; private set; }

        /// <summary>
        /// Moves the report to another status, such as after the automatic check.
        /// </summary>
        /// <param name="reportStatusId">Identifier of the new status; must be positive.</param>
        public void ChangeStatus(int reportStatusId)
        {
            ValidateId(reportStatusId, nameof(reportStatusId));

            ReportStatusId = reportStatusId;
        }

        // Both ids reference accounts that already exist, and nobody reports themselves.
        private static void ValidatePlayers(int reporterPlayerId, int reportedPlayerId)
        {
            ValidateId(reporterPlayerId, nameof(reporterPlayerId));
            ValidateId(reportedPlayerId, nameof(reportedPlayerId));

            if (reporterPlayerId == reportedPlayerId)
            {
                throw new ArgumentException("A player cannot report themselves.", nameof(reportedPlayerId));
            }
        }

        // The report references rows that already exist, so their ids must be positive.
        private static void ValidateId(int id, string parameterName)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Id must be positive.", parameterName);
            }
        }
    }
}
