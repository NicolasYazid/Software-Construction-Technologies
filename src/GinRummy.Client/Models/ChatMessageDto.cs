namespace GinRummy.Client.Models
{
    // The content travels untranslated and uncensored because each reader decides whether to see it or the censored mark (CU-17).
    public sealed class ChatMessageDto : ChatEntryDto
    {
        public string AuthorName { get; set; }
        public string Content { get; set; }
        public bool IsCensored { get; set; }
    }
}
