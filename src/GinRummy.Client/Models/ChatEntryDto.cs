using System;

namespace GinRummy.Client.Models
{
    /// <summary>
    /// Entry of the lobby chat. The chat interleaves the messages of the players with the
    /// notices of the challenges, so both derive from this type and each one is drawn by its
    /// own template.
    /// </summary>
    public abstract class ChatEntryDto
    {
        /// <summary>
        /// Gets or sets the moment the entry reached the lobby.
        /// </summary>
        public DateTime SentAt { get; set; }
    }
}
