using System.ServiceModel;

using GinRummy.Contracts.Faults.Shared;

namespace GinRummy.Contracts.Services.SocialCommunication
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
