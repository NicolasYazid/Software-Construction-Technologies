using System.Collections.Generic;
using System.ServiceModel;

using GinRummy.Contracts.DataContracts;

namespace GinRummy.Contracts.Callbacks
{
    public interface IChatMessageCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnLobbyChatHistoryReceived(List<PlayerMessage> messages);

        [OperationContract(IsOneWay = true)]
        void OnChatMessageReceived(PlayerMessage message);
    }
}
