using System.Collections.Generic;
using System.ServiceModel;

using GinRummy.Contracts.DataContracts.SocialCommunication;

namespace GinRummy.Contracts.Services.SocialCommunication
{
    public interface IChatMessageCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnLobbyChatHistoryReceived(List<PlayerMessage> messages);

        [OperationContract(IsOneWay = true)]
        void OnChatMessageReceived(PlayerMessage message);
    }
}
