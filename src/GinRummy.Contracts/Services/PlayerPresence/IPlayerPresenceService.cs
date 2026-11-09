using System.ServiceModel;

using GinRummy.Contracts.Faults.Shared;

namespace GinRummy.Contracts.Services.PlayerPresence
{
    [ServiceContract(CallbackContract = typeof(IPlayerPresenceCallback), SessionMode = SessionMode.Required)]
    public interface IPlayerPresenceService
    {
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void SubscribeToPlayerPresence(string accessToken);

        [OperationContract(IsOneWay = true)]
        void UnsubscribeFromPlayerPresence();
    }
}
