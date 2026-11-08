using System;

using GinRummy.Domain.Entities.Players;

namespace GinRummy.Domain.Entities.SocialCommunication
{
    public class Message
    {
        public const int ContentMaxLength = 300;
        public const int ChannelNameMaxLength = 20;

        public Message(int playerId, string content, MessageChannel channel, int? gameSessionId, DateTime sentAt)
        {
            ValidatePlayerId(playerId);
            ValidateContent(content);
            ValidateChannel(channel, gameSessionId);

            PlayerId = playerId;
            Content = content;
            ChannelName = channel.ToString();
            GameSessionId = gameSessionId;
            SentAt = sentAt;
        }

        protected Message()
        {
        }

        public int MessageId { get; private set; }
        public int PlayerId { get; private set; }
        public string Content { get; private set; }
        public string ChannelName { get; private set; }
        public int? GameSessionId { get; private set; }
        public DateTime SentAt { get; private set; }
        public virtual Player Player { get; private set; }

        public MessageChannel Channel
        {
            get { return (MessageChannel)Enum.Parse(typeof(MessageChannel), ChannelName); }
        }

        private static void ValidatePlayerId(int playerId)
        {
            if (playerId <= 0)
            {
                throw new ArgumentException("Player id must be positive.", nameof(playerId));
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
