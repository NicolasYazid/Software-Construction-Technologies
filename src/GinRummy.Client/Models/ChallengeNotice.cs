using System;

namespace GinRummy.Client.Models
{
    public sealed class ChallengeNotice : ChatEntry
    {
        public ChallengeNotice(ChallengeNoticeKind kind, string playerName, DateTime sentAt)
            : base(sentAt)
        {
            Kind = kind;
            PlayerName = playerName ?? throw new ArgumentNullException(nameof(playerName));
        }

        public ChallengeNoticeKind Kind { get; private set; }
        public string PlayerName { get; private set; }
    }
}
