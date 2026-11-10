using System.ServiceModel;

using GinRummy.Contracts.Faults.Shared;

namespace GinRummy.Contracts.Services.DirectDuel
{
    [ServiceContract(CallbackContract = typeof(IDirectDuelCallback), SessionMode = SessionMode.Required)]
    public interface IDirectDuelService
    {
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void SubscribeToDirectDuel(string accessToken);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void ChallengePlayer(string accessToken, int challengedPublicTag);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void CancelChallenge(string accessToken);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void AcceptChallenge(string accessToken, int challengerPublicTag);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void DeclineChallenge(string accessToken, int challengerPublicTag);

        [OperationContract(IsOneWay = true)]
        void UnsubscribeFromDirectDuel();
    }
}
