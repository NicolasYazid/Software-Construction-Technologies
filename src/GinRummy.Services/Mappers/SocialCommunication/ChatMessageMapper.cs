using System;
using System.Collections.Generic;

using GinRummy.Application.UseCases.SocialCommunication;
using GinRummy.Contracts.DataContracts.SocialCommunication;
using GinRummy.Domain.Entities.PlayerCustomization;
using GinRummy.Domain.Entities.SocialCommunication;

namespace GinRummy.Services.Mappers.SocialCommunication
{
    public static class ChatMessageMapper
    {
        private const int NoAddresseePublicTag = 0;

        public static PlayerMessage MapToPlayerMessage(SentChatMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            PlayerProfile author = message.AuthorProfile;

            return new PlayerMessage
            {
                MessageId = message.MessageId,
                Destination = MapToMessageDestination(message.Channel),
                Username = author.Username,
                PublicTag = author.PublicTag,
                AddresseePublicTag = FindAddresseePublicTag(message),
                ProfilePicture = message.AuthorProfile.ProfilePicture,
                Content = message.Content,
                SentAt = message.SentAt
            };
        }

        public static List<PlayerMessage> MapToPlayerMessages(List<SentChatMessage> messages)
        {
            if (messages == null)
            {
                throw new ArgumentNullException(nameof(messages));
            }

            List<PlayerMessage> playerMessages = new List<PlayerMessage>();
            foreach (SentChatMessage message in messages)
            {
                playerMessages.Add(MapToPlayerMessage(message));
            }

            return playerMessages;
        }

        public static SendMessageRequest MapToSendMessageRequest(MessageToSend message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            MessageChannel channel = (MessageChannel)Enum.Parse(typeof(MessageChannel), message.Destination.ToString());

            return new SendMessageRequest(channel, message.Content, message.FriendPublicTag);
        }

        private static MessageDestination MapToMessageDestination(MessageChannel channel)
        {
            return (MessageDestination)Enum.Parse(typeof(MessageDestination), channel.ToString());
        }

        private static int FindAddresseePublicTag(SentChatMessage message)
        {
            return (message.AddresseeProfile != null) ? message.AddresseeProfile.PublicTag : NoAddresseePublicTag;
        }
    }
}
