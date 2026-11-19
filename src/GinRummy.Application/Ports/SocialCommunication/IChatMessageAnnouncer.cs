using System.Collections.Generic;

using GinRummy.Application.UseCases.SocialCommunication;

namespace GinRummy.Application.Ports.SocialCommunication
{
    public interface IChatMessageAnnouncer
    {
        void AnnounceMessageToAllPlayers(SentChatMessage message);

        void SendMessageToAuthorAndAddressee(int authorId, int addresseeId, SentChatMessage message);

        void SendLobbyHistoryToPlayer(int playerId, List<SentChatMessage> messages);

        bool IsPlayerOnline(int playerId);
    }
}
