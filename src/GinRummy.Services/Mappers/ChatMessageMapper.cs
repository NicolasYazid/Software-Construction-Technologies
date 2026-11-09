using System;
using System.Collections.Generic;

using GinRummy.Application.UseCases.SocialCommunication;
using GinRummy.Contracts.DataContracts;
using GinRummy.Domain.Entities.PlayerCustomization;
using GinRummy.Domain.Entities.SocialCommunication;

namespace GinRummy.Services.Mappers
{
    public static class ChatMessageMapper
    {
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

        private static MessageDestination MapToMessageDestination(MessageChannel channel)
        {
            return (channel == MessageChannel.Match) ? MessageDestination.Match : MessageDestination.Lobby;
        }
    }
}
