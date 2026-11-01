using System;

using GinRummy.Domain.Entities.Players;

namespace GinRummy.Domain.Entities.ChatMessaging
{
    /// <summary>
    /// A chat message, mapped to the Message table. The text is stored exactly as the player
    /// wrote it, because moderation checks it. Protects its own invariants: a
    /// lobby message belongs to no match and a match message belongs to one.
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Length of the content column.
        /// </summary>
        public const int ContentMaxLength = 300;

        /// <summary>
        /// Length of the channel column.
        /// </summary>
        public const int ChannelNameMaxLength = 20;

        /// <summary>
        /// Creates a valid new message.
        /// </summary>
        /// <param name="playerId">Identifier of the author; must be positive.</param>
        /// <param name="content">Text as written; required, at most 300 characters.</param>
        /// <param name="channel">Where the message was written.</param>
        /// <param name="gameSessionId">Identifier of the match for a match message; null for a lobby message.</param>
        /// <param name="sentAt">Moment the message was sent, in UTC.</param>
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

        /// <summary>
        /// Parameterless constructor reserved for Entity Framework's materialization; not
        /// for application code, which must use the validating constructor.
        /// </summary>
        protected Message()
        {
        }

        /// <summary>
        /// Gets the internal, autonumeric identifier.
        /// </summary>
        public int MessageId { get; private set; }

        /// <summary>
        /// Gets the identifier of the author.
        /// </summary>
        public int PlayerId { get; private set; }

        /// <summary>
        /// Gets the text exactly as the author wrote it. It is never stored censored.
        /// </summary>
        public string Content { get; private set; }

        /// <summary>
        /// Gets the channel exactly as the channel column stores it.
        /// </summary>
        public string ChannelName { get; private set; }

        /// <summary>
        /// Gets the identifier of the match the message belongs to. Null for a lobby message.
        /// </summary>
        public int? GameSessionId { get; private set; }

        /// <summary>
        /// Gets the moment the message was sent, in UTC.
        /// </summary>
        public DateTime SentAt { get; private set; }

        /// <summary>
        /// Gets the author, loaded from the relationship between the two tables.
        /// </summary>
        public virtual Player Player { get; private set; }

        /// <summary>
        /// Gets where the message was written.
        /// </summary>
        public MessageChannel Channel
        {
            get { return (MessageChannel)Enum.Parse(typeof(MessageChannel), ChannelName); }
        }

        // A message always has an author, referenced by a positive id.
        private static void ValidatePlayerId(int playerId)
        {
            if (playerId <= 0)
            {
                throw new ArgumentException("Player id must be positive.", nameof(playerId));
            }
        }

        // An empty message is never stored, and the text must fit the column.
        // The text is not trimmed, so it stays as the author wrote it.
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

        // The Message table rejects a match message without a match and a lobby message with one.
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
