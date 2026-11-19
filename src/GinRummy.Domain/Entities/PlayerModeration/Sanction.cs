using System;

using GinRummy.Domain.Entities.SocialCommunication;

namespace GinRummy.Domain.Entities.PlayerModeration
{
    public class Sanction
    {
        public const int DurationMinutes = 5;
        public const int KeyMaxLength = 100;
        public const string OffensiveLanguageReasonKey = "Moderation_SanctionReason";
        public const string LobbyAndMatchMessageKey = "Moderation_SanctionMessageLobbyAndMatch";
        public const string AllChatsMessageKey = "Moderation_SanctionMessageAllChats";

        public Sanction(Warning triggeringWarning, MessageChannel origin, SanctionSchedule schedule)
        {
            if (triggeringWarning == null)
            {
                throw new ArgumentNullException(nameof(triggeringWarning));
            }

            if (triggeringWarning.WarningId <= 0)
            {
                throw new ArgumentException("The warning must be saved before it triggers a sanction.", nameof(triggeringWarning));
            }

            if (!Enum.IsDefined(typeof(MessageChannel), origin))
            {
                throw new ArgumentOutOfRangeException(nameof(origin), "Origin is not valid.");
            }

            if (schedule == null)
            {
                throw new ArgumentNullException(nameof(schedule));
            }

            PlayerAccountId = triggeringWarning.PlayerAccountId;
            WarningId = triggeringWarning.WarningId;
            ReasonKey = OffensiveLanguageReasonKey;
            MessageKey = (origin == MessageChannel.Friend) ? AllChatsMessageKey : LobbyAndMatchMessageKey;
            CreatedAt = schedule.CreatedAt;
            StartsAt = schedule.StartsAt;
            EndsAt = schedule.EndsAt;
        }

        protected Sanction()
        {
        }

        public int SanctionId { get; private set; }
        public int PlayerAccountId { get; private set; }
        public int WarningId { get; private set; }
        public string ReasonKey { get; private set; }
        public string MessageKey { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime StartsAt { get; private set; }
        public DateTime EndsAt { get; private set; }

        public bool BlocksAllChats
        {
            get { return MessageKey == AllChatsMessageKey; }
        }

        public bool IsActiveAt(DateTime moment)
        {
            bool isActive = (StartsAt <= moment) && (moment < EndsAt) ? true : false;

            return isActive;
        }

        public bool Blocks(MessageChannel channel)
        {
            bool isBlocked = BlocksAllChats || (channel != MessageChannel.Friend) ? true : false;

            return isBlocked;
        }
    }
}
