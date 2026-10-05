namespace GinRummy.Client.Models
{
    // Notice of a challenge shown among the messages of the lobby chat.
    public sealed class ChallengeNoticeDto : ChatEntryDto
    {
        public ChallengeNoticeKind Kind { get; set; }
        public string PlayerName { get; set; }
    }
}
