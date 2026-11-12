using System.ServiceModel;

using GinRummy.Contracts.DataContracts.DirectDuel;
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
        ChallengeOutcome ChallengePlayer(string accessToken, int challengedPublicTag);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void CancelChallenge(string accessToken);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ChallengeOutcome AcceptChallenge(string accessToken, int challengerPublicTag);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ChallengeOutcome DeclineChallenge(string accessToken, int challengerPublicTag);

        [OperationContract(IsOneWay = true)]
        void UnsubscribeFromDirectDuel();
    }
}
