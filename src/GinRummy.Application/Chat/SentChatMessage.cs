using System;

using GinRummy.Domain.Entities.Players;
using GinRummy.Domain.Entities.SocialCommunication;

namespace GinRummy.Application.Chat
{
    public class SentChatMessage
    {
        private SentChatMessage(PlayerProfile authorProfile, string content, int? gameSessionId)
        {
            MessageId = Guid.NewGuid();
            AuthorProfile = authorProfile;
            Content = content;
            GameSessionId = gameSessionId;
        }

        public Guid MessageId { get; private set; }
        public PlayerProfile AuthorProfile { get; private set; }
        public string Content { get; private set; }
        public int? GameSessionId { get; private set; }
        public DateTime SentAt { get; private set; }

        public MessageChannel Channel
        {
            get { return GameSessionId.HasValue ? MessageChannel.Match : MessageChannel.Lobby; }
        }

        public static SentChatMessage ForLobby(PlayerProfile authorProfile, string content)
        {
            if (authorProfile == null)
            {
                throw new ArgumentNullException(nameof(authorProfile));
            }

            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            return new SentChatMessage(authorProfile, content, null);
        }

        public static SentChatMessage ForMatch(PlayerProfile authorProfile, string content, int gameSessionId)
        {
            if (authorProfile == null)
            {
                throw new ArgumentNullException(nameof(authorProfile));
            }

            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            return new SentChatMessage(authorProfile, content, gameSessionId);
        }

        public void AssignSentAt(DateTime sentAt)
        {
            SentAt = sentAt;
        }
    }
}
