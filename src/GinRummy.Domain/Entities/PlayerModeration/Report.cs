using System;

namespace GinRummy.Domain.Entities.PlayerModeration
{
    /// <summary>
    /// A report a player makes against the author of a chat message, mapped to the Report table.
    /// It is born pending, because the review of the reported message decides it later. Protects
    /// its own invariants: it references real rows and a player never reports themselves.
    /// </summary>
    public class Report
    {
        /// <summary>
        /// Identifier of the Pending status in the ReportStatus catalog.
        /// </summary>
        public const int PendingStatusId = 1;

        /// <summary>
        /// Creates a valid new pending report. The database sets the moment it was created.
        /// </summary>
        /// <param name="reporterPlayerAccountId">Identifier of the player who reports; must be positive.</param>
        /// <param name="reportedPlayerAccountId">Identifier of the author of the message; must be positive and not the reporter.</param>
        /// <param name="messageId">Identifier of the reported message; must be positive.</param>
        public Report(int reporterPlayerAccountId, int reportedPlayerAccountId, int messageId)
        {
            ValidateId(reporterPlayerAccountId, nameof(reporterPlayerAccountId));
            ValidateId(reportedPlayerAccountId, nameof(reportedPlayerAccountId));
            ValidateId(messageId, nameof(messageId));

            if (reporterPlayerAccountId == reportedPlayerAccountId)
            {
                throw new ArgumentException("A player cannot report their own message.", nameof(reportedPlayerAccountId));
            }

            ReporterPlayerAccountId = reporterPlayerAccountId;
            ReportedPlayerAccountId = reportedPlayerAccountId;
            MessageId = messageId;
            ReportStatusId = PendingStatusId;
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
        /// Gets the identifier of the player who reports.
        /// </summary>
        public int ReporterPlayerAccountId { get; private set; }

        /// <summary>
        /// Gets the identifier of the author of the reported message.
        /// </summary>
        public int ReportedPlayerAccountId { get; private set; }

        /// <summary>
        /// Gets the identifier of the reported message.
        /// </summary>
        public int MessageId { get; private set; }

        /// <summary>
        /// Gets the identifier of the status of the report in the ReportStatus catalog.
        /// </summary>
        public int ReportStatusId { get; private set; }

        /// <summary>
        /// Gets the moment the report was created, in UTC.
        /// </summary>
        public DateTime CreatedAt { get; private set; }

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
