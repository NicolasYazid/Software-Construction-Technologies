namespace GinRummy.Client.Models
{
    // Message a player wrote in the lobby chat. The content travels as its author wrote it and
    // is never translated; whether a reader sees it or the censored mark is decided for each
    // reader (CU-17).
    public sealed class ChatMessageDto : ChatEntryDto
    {
        public string AuthorName { get; set; }
        public string Content { get; set; }
        public bool IsCensored { get; set; }
    }
}
