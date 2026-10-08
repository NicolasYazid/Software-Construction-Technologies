namespace GinRummy.Client.Models
{
    public sealed class ChallengeNoticeDto : ChatEntryDto
    {
        public ChallengeNoticeKind Kind { get; set; }
        public string PlayerName { get; set; }
    }
}
