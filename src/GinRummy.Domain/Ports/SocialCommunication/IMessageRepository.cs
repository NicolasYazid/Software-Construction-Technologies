using GinRummy.Domain.Entities.SocialCommunication;

namespace GinRummy.Domain.Ports.SocialCommunication
{
    public interface IMessageRepository
    {
        Message FindById(int messageId);

        void Add(Message newMessage);
    }
}
