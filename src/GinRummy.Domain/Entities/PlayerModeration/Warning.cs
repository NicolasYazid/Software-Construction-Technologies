using System;

namespace GinRummy.Domain.Entities.PlayerModeration
{
    public class Warning
    {
        public Warning(int playerAccountId, int reportId, DateTime issuedAt)
        {
            ValidateId(playerAccountId, nameof(playerAccountId));
            ValidateId(reportId, nameof(reportId));

            PlayerAccountId = playerAccountId;
            ReportId = reportId;
            IssuedAt = issuedAt;
        }

        protected Warning()
        {
        }

        public int WarningId { get; private set; }
        public int PlayerAccountId { get; private set; }
        public int ReportId { get; private set; }
        public DateTime IssuedAt { get; private set; }
        public virtual Report Report { get; private set; }

        private static void ValidateId(int id, string parameterName)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Id must be positive.", parameterName);
            }
        }
    }
}
