using System;

using GinRummy.Domain.Entities.PlayerCustomization;

namespace GinRummy.Domain.Entities.SocialCommunication
{
    public class Message
    {
        public const int ContentMaxLength = 300;
        public const int ChannelNameMaxLength = 20;

        public Message(int playerAccountId, string content, MessageChannel channel, int? gameSessionId, DateTime sentAt)
        {
            ValidatePlayerAccountId(playerAccountId);
            ValidateContent(content);
            ValidateChannel(channel, gameSessionId);

            PlayerAccountId = playerAccountId;
            Content = content;
            ChannelName = channel.ToString();
            GameSessionId = gameSessionId;
            SentAt = sentAt;
        }

        protected Message()
        {
        }

        public int MessageId { get; private set; }
        public int PlayerAccountId { get; private set; }
        public string Content { get; private set; }
        public string ChannelName { get; private set; }
        public int? GameSessionId { get; private set; }
        public DateTime SentAt { get; private set; }
        public virtual PlayerProfile PlayerProfile { get; private set; }

        public MessageChannel Channel
        {
            get { return (MessageChannel)Enum.Parse(typeof(MessageChannel), ChannelName); }
        }

        private static void ValidatePlayerAccountId(int playerAccountId)
        {
            if (playerAccountId <= 0)
            {
                throw new ArgumentException("Player account id must be positive.", nameof(playerAccountId));
            }
        }

        private static void ValidateContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException("Content is required.", nameof(content));
            }

            if (content.Length > ContentMaxLength)
            {
                throw new ArgumentException("Content is too long.", nameof(content));
            }
        }

        private static void ValidateChannel(MessageChannel channel, int? gameSessionId)
        {
            if (!Enum.IsDefined(typeof(MessageChannel), channel))
            {
                throw new ArgumentOutOfRangeException(nameof(channel), "Channel is not valid.");
            }

            bool isMatchWithoutSession = (channel == MessageChannel.Match) && !gameSessionId.HasValue;
            bool isLobbyWithSession = (channel == MessageChannel.Lobby) && gameSessionId.HasValue;
            if (isMatchWithoutSession || isLobbyWithSession)
            {
                throw new ArgumentException("The game session must be set only for match messages.", nameof(gameSessionId));
            }
        }
    }
}
