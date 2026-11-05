using System;

namespace GinRummy.Client.Models
{
    // The content travels untranslated and uncensored because each reader decides whether to see it or the censored mark (CU-17).
    public sealed class ChatMessage
    {
        public ChatMessage(string authorName, string content, DateTime sentAt)
        {
            AuthorName = authorName ?? throw new ArgumentNullException(nameof(authorName));
            Content = content ?? throw new ArgumentNullException(nameof(content));
            SentAt = sentAt;
        }

        public string AuthorName { get; private set; }
        public string Content { get; private set; }
        public DateTime SentAt { get; private set; }
        public bool IsCensored { get; private set; }

        public void Censor()
        {
            IsCensored = true;
        }
    }
}
