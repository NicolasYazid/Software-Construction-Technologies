using System;

namespace GinRummy.Client.Models
{
    // Entry of the lobby chat.
    // The chat interleaves the messages of the players with the notices of the challenges.
    // Both derive from this type, and each one is drawn by its own template.
    public abstract class ChatEntryDto
    {
        public DateTime SentAt { get; set; }
    }
}
