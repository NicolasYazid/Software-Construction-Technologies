using System.ServiceModel;

using GinRummy.Contracts.DataContracts.SocialCommunication;
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
        void SendMessage(string accessToken, MessageToSend message);

        [OperationContract(IsOneWay = true)]
        void UnsubscribeFromChat();
    }
}
