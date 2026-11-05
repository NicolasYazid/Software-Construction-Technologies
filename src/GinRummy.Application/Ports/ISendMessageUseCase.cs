namespace GinRummy.Application.Ports
{
    public interface ISendMessageUseCase
    {
        void SendLobbyMessage(int authorId, string content);

        void SendMatchMessage(int authorId, string content);

        void SendLobbyHistoryToPlayer(int playerId);
    }
}
