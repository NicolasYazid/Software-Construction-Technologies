using System;
using System.Runtime.Serialization;

namespace GinRummy.Contracts.DataContracts
{
    /// <summary>
    /// Chat message as the client receives it, with the data of its author that the chat shows next to the text.
    /// </summary>
    [DataContract]
    public class PlayerMessage
    {
        /// <summary>
        /// Identifier the client sends back to report this message.
        /// </summary>
        [DataMember]
        public Guid MessageId { get; set; }

        /// <summary>
        /// Chat where the client shows the message.
        /// </summary>
        [DataMember]
        public MessageDestination Destination { get; set; }

        /// <summary>
        /// Display name of the author.
        /// </summary>
        [DataMember]
        public string Username { get; set; }

        /// <summary>
        /// Public tag of the author, which tells apart two players with the same display name.
        /// </summary>
        [DataMember]
        public int PublicTag { get; set; }

        /// <summary>
        /// Profile picture of the author; null when the author has none and the client shows the default avatar.
        /// </summary>
        [DataMember]
        public string ProfilePicture { get; set; }

        /// <summary>
        /// Text exactly as the author wrote it.
        /// </summary>
        [DataMember]
        public string Content { get; set; }

        /// <summary>
        /// Moment the server received the message, in UTC; the client turns it into local time to show it.
        /// </summary>
        [DataMember]
        public DateTime SentAt { get; set; }
    }
}
