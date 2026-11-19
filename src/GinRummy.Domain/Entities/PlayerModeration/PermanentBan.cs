using System;

namespace GinRummy.Domain.Entities.PlayerModeration
{
    public class PermanentBan
    {
        public const int KeyMaxLength = 100;
        public const string ConductViolationReasonKey = "Moderation_BanReason";
        public const string BanMessageKey = "Moderation_BanMessage";

        public PermanentBan(Sanction triggeringSanction, DateTime createdAt)
        {
            if (triggeringSanction == null)
            {
                throw new ArgumentNullException(nameof(triggeringSanction));
            }

            if (triggeringSanction.SanctionId <= 0)
            {
                throw new ArgumentException("The sanction must be saved before it triggers a ban.", nameof(triggeringSanction));
            }

            PlayerAccountId = triggeringSanction.PlayerAccountId;
            SanctionId = triggeringSanction.SanctionId;
            ReasonKey = ConductViolationReasonKey;
            MessageKey = BanMessageKey;
            CreatedAt = createdAt;
        }

        protected PermanentBan()
        {
        }

        public int PermanentBanId { get; private set; }
        public int PlayerAccountId { get; private set; }
        public int SanctionId { get; private set; }
        public string ReasonKey { get; private set; }
        public string MessageKey { get; private set; }
        public DateTime CreatedAt { get; private set; }
    }
}
