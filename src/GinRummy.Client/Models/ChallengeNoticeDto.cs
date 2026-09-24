namespace GinRummy.Client.Models
{
    /// <summary>
    /// Notice of a challenge shown among the messages of the lobby chat.
    /// </summary>
    public sealed class ChallengeNoticeDto : ChatEntryDto
    {
        /// <summary>
        /// Gets or sets which of the notices of a challenge this one is.
        /// </summary>
        public ChallengeNoticeKind Kind { get; set; }
        /// <summary>
        /// Gets or sets the name of the other player of the challenge.
        /// </summary>
        public string PlayerName { get; set; }
    }
}
