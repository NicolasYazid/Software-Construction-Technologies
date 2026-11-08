using System.ServiceModel;

using GinRummy.Contracts.Callbacks;
using GinRummy.Contracts.Faults;

namespace GinRummy.Contracts.Services
{
    [ServiceContract(CallbackContract = typeof(IChatMessageCallback), SessionMode = SessionMode.Required)]
    public interface IChatMessageService
    {
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void SubscribeToChat(string accessToken);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void SendLobbyMessage(string accessToken, string content);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void SendMatchMessage(string accessToken, string content);

        [OperationContract(IsOneWay = true)]
        void UnsubscribeFromChat();
    }
}
