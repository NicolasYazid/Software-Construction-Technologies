using System.ServiceModel;

namespace GinRummy.Contracts.Services.DirectDuel
{
    public interface IChallengeCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnChallengeReceived(string challengerName, int challengerPublicTag);

        [OperationContract(IsOneWay = true)]
        void OnChallengeCancelled(int challengerPublicTag);

        [OperationContract(IsOneWay = true)]
        void OnChallengeDeclined(int challengedPublicTag);

        [OperationContract(IsOneWay = true)]
        void OnRoundReady();
    }
}
