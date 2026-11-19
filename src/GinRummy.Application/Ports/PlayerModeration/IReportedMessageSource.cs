using GinRummy.Domain.Entities.SocialCommunication;

namespace GinRummy.Application.Ports.PlayerModeration
{
    public interface IReportedMessageSource
    {
        Message SaveReportedMessage(int chatMessageId);
    }
}
