using System.Collections.Generic;

using GinRummy.Application.Chat;

namespace GinRummy.Application.Ports
{
    public interface IChatMessageAnnouncer
    {
        void AnnounceMessageToAllPlayers(SentChatMessage message);

        void SendMessageToMatchPlayers(int authorId, int rivalId, SentChatMessage message);

        void SendLobbyHistoryToPlayer(int playerId, List<SentChatMessage> messages);
    }
}
