using System.ServiceModel;

using GinRummy.Contracts.DataContracts;

namespace GinRummy.Contracts.Callbacks
{
    public interface IPlayerPresenceCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnCurrentStatusOfPlayersReported(PlayerPresenceReport report);

        [OperationContract(IsOneWay = true)]
        void OnPlayerPresenceChanged(PlayerPresence presence);
    }
}
