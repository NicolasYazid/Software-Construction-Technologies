using System;

namespace GinRummy.Domain.Entities.PlayerModeration
{
    public class Report
    {
        public const int PendingStatusId = 1;

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

        protected Report()
        {
        }

        public int ReportId { get; private set; }

        public int ReporterPlayerAccountId { get; private set; }

        public int ReportedPlayerAccountId { get; private set; }

        public int MessageId { get; private set; }

        public int ReportStatusId { get; private set; }

        public DateTime CreatedAt { get; private set; }

        private static void ValidateId(int id, string parameterName)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Id must be positive.", parameterName);
            }
        }
    }
}
