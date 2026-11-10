using System.ServiceModel;

using GinRummy.Contracts.DataContracts.Gameplay;

namespace GinRummy.Contracts.Services.Gameplay
{
    public interface IGameCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnRoundChanged(RoundDetails round);

        [OperationContract(IsOneWay = true)]
        void OnRoundFinished(RoundSummary summary);

        [OperationContract(IsOneWay = true)]
        void OnOpponentLeft();
    }
}
