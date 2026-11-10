using System.ServiceModel;

using GinRummy.Contracts.DataContracts.Gameplay;
using GinRummy.Contracts.Faults.Shared;

namespace GinRummy.Contracts.Services.Gameplay
{
    [ServiceContract(CallbackContract = typeof(IGameCallback), SessionMode = SessionMode.Required)]
    public interface IGameService
    {
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void SubscribeToGame(string accessToken);

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

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void TakeFirstCard(string accessToken);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void PassFirstCard(string accessToken);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void DrawFromStock(string accessToken);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void DrawFromDiscardPile(string accessToken);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void Discard(string accessToken, PlayingCard card);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void Knock(string accessToken, PlayingCard discardedCard);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void ForfeitRound(string accessToken);

        [OperationContract(IsOneWay = true)]
        void UnsubscribeFromGame();
    }
}
