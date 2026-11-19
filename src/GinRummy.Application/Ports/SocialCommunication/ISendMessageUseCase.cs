using GinRummy.Application.UseCases.SocialCommunication;

namespace GinRummy.Application.Ports.SocialCommunication
{
    public interface ISendMessageUseCase
    {
        void SendMessage(int authorId, SendMessageRequest request);

        void SendLobbyHistoryToPlayer(int playerId);
    }
}
