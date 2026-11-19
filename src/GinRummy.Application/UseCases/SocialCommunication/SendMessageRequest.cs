using GinRummy.Domain.Entities.SocialCommunication;

namespace GinRummy.Application.UseCases.SocialCommunication
{
    public class SendMessageRequest
    {
        public SendMessageRequest(MessageChannel channel, string content, int friendPublicTag)
        {
            Channel = channel;
            Content = content;
            FriendPublicTag = friendPublicTag;
        }

        public MessageChannel Channel { get; private set; }
        public string Content { get; private set; }
        public int FriendPublicTag { get; private set; }
    }
}
