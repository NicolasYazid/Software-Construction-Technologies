namespace GinRummy.Application.Ports
{
    public interface IMessageReportPort
    {
        bool ReportMessage(string accessToken, int chatMessageId);
    }
}
