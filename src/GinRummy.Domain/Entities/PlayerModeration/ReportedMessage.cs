using System;

using GinRummy.Domain.Entities.SocialCommunication;

namespace GinRummy.Domain.Entities.PlayerModeration
{
    public class ReportedMessage
    {
        public const int ContentMaxLength = 300;

        public ReportedMessage(string content, DateTime sentAt, MessageChannel origin)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException("Content is required.", nameof(content));
            }

            if (content.Length > ContentMaxLength)
            {
                throw new ArgumentException("Content is too long.", nameof(content));
            }

            if (!Enum.IsDefined(typeof(MessageChannel), origin))
            {
                throw new ArgumentOutOfRangeException(nameof(origin), "Origin is not valid.");
            }

            Content = content;
            SentAt = sentAt;
            Origin = origin;
        }

        public string Content { get; private set; }
        public DateTime SentAt { get; private set; }
        public MessageChannel Origin { get; private set; }
    }
}
