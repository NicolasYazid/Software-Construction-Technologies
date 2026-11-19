namespace GinRummy.Domain.Entities.SocialCommunication
{
    public class MessageDraft
    {
        public MessageDraft(string content, MessageChannel channel, int? gameSessionId)
        {
            Content = content;
            Channel = channel;
            GameSessionId = gameSessionId;
        }

        public string Content { get; private set; }

        public MessageChannel Channel { get; private set; }

        public int? GameSessionId { get; private set; }
    }
}
