namespace GinRummy.Client.Models
{
    public sealed class ChallengeNotice : ChatEntry
    {
        public ChallengeNoticeKind Kind { get; set; }
        public string PlayerName { get; set; }
    }
}
