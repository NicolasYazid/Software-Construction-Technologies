using System;

using GinRummy.Domain.Entities.SocialCommunication;

namespace GinRummy.Domain.Entities.PlayerModeration
{
    public class Report
    {
        public const int OriginNameMaxLength = 20;

        public Report(int reporterPlayerAccountId, int reportedPlayerAccountId, ReportedMessage message)
        {
            ValidateId(reporterPlayerAccountId, nameof(reporterPlayerAccountId));
            ValidateId(reportedPlayerAccountId, nameof(reportedPlayerAccountId));

            if (reporterPlayerAccountId == reportedPlayerAccountId)
            {
                throw new ArgumentException("A player cannot report their own message.", nameof(reportedPlayerAccountId));
            }

            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            ReporterPlayerAccountId = reporterPlayerAccountId;
            ReportedPlayerAccountId = reportedPlayerAccountId;
            ReportedContent = message.Content;
            MessageSentAt = message.SentAt;
            OriginName = message.Origin.ToString();
        }

        protected Report()
        {
        }

        public int ReportId { get; private set; }
        public int ReporterPlayerAccountId { get; private set; }
        public int ReportedPlayerAccountId { get; private set; }
        public string ReportedContent { get; private set; }
        public DateTime MessageSentAt { get; private set; }
        public string OriginName { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public MessageChannel Origin
        {
            get { return (MessageChannel)Enum.Parse(typeof(MessageChannel), OriginName); }
        }

        private static void ValidateId(int id, string parameterName)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Id must be positive.", parameterName);
            }
        }
    }
}
