using System.ServiceModel;

using GinRummy.Contracts.Callbacks;
using GinRummy.Contracts.Faults;

namespace GinRummy.Contracts.Services
{
    [ServiceContract(CallbackContract = typeof(IPlayerPresenceCallback), SessionMode = SessionMode.Required)]
    public interface IPlayerPresenceService
    {
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void SubscribeToPlayerPresence(string accessToken);

        void UnsubscribeFromPlayerPresence();
    }
}
