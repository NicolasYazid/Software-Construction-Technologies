using GinRummy.Domain.Entities.ChatMessaging;

namespace GinRummy.Application.Ports
{
    // The chat keeps its latest messages only in memory, so it saves a message in the database once someone reports it.
    // It returns null when the message is no longer among the ones it keeps.
    public interface IReportedMessageSource
    {
        Message SaveReportedMessage(int chatMessageId);
    }
}
