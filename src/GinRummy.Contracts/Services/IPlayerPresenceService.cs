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

        /// <summary>
        /// Stops the notices of the presence of the players. The other players see this player as disconnected.
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void UnsubscribeFromPlayerPresence();
    }
}
