namespace GinRummy.Client.Models
{
    // A report asks nothing of the player, so the chat only confirms that it was received (CU-18).
    public sealed class ReportNoticeDto : ChatEntryDto
    {
        public string PlayerName { get; set; }
    }
}
