namespace GinRummy.Application.Ports.PlayerModeration
{
    public interface IMessageReportPort
    {
        bool ReportMessage(string accessToken, int chatMessageId);
    }
}
