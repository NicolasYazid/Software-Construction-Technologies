namespace GinRummy.Client.Models
{
    /// <summary>
    /// Message a player wrote in the lobby chat. The content travels as its author wrote it
    /// and is never translated; whether a reader sees it or the censored mark is decided for
    /// each reader (CU-17).
    /// </summary>
    public sealed class ChatMessageDto : ChatEntryDto
    {
        /// <summary>
        /// Gets or sets the name of the player who wrote the message.
        /// </summary>
        public string AuthorName { get; set; }
        /// <summary>
        /// Gets or sets the text of the message as its author wrote it.
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// Gets or sets whether the reader receives the censored mark instead of the text.
        /// </summary>
        public bool IsCensored { get; set; }
    }
}
