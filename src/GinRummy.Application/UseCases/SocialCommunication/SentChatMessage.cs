using System;

using GinRummy.Domain.Entities.PlayerCustomization;
using GinRummy.Domain.Entities.SocialCommunication;

namespace GinRummy.Application.UseCases.SocialCommunication
{
    public class SentChatMessage
    {
        private SentChatMessage(PlayerProfile authorProfile, string content, MessageChannel channel)
        {
            MessageId = Guid.NewGuid();
            AuthorProfile = authorProfile;
            Content = content;
            Channel = channel;
        }

        public Guid MessageId { get; private set; }
        public PlayerProfile AuthorProfile { get; private set; }
        public string Content { get; private set; }
        public MessageChannel Channel { get; private set; }
        public int? GameSessionId { get; private set; }
        public PlayerProfile AddresseeProfile { get; private set; }
        public DateTime SentAt { get; private set; }

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

            return new SentChatMessage(authorProfile, content, MessageChannel.Lobby);
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

            SentChatMessage matchMessage = new SentChatMessage(authorProfile, content, MessageChannel.Match);
            matchMessage.GameSessionId = gameSessionId;

            return matchMessage;
        }

        public static SentChatMessage ForFriend(PlayerProfile authorProfile, string content, PlayerProfile friendProfile)
        {
            if (authorProfile == null)
            {
                throw new ArgumentNullException(nameof(authorProfile));
            }

            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            if (friendProfile == null)
            {
                throw new ArgumentNullException(nameof(friendProfile));
            }

            SentChatMessage friendMessage = new SentChatMessage(authorProfile, content, MessageChannel.Friend);
            friendMessage.AddresseeProfile = friendProfile;

            return friendMessage;
        }

        public void AssignSentAt(DateTime sentAt)
        {
            SentAt = sentAt;
        }
    }
}
