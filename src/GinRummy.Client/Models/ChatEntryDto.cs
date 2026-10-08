using System;

namespace GinRummy.Client.Models
{
    // Messages and challenge notices share this base type so that the chat interleaves them in one list, each with its own template.
    public abstract class ChatEntryDto
    {
        public DateTime SentAt { get; set; }
    }
}
