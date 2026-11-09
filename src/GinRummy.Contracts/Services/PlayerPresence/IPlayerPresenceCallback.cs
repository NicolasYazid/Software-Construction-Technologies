using System.ServiceModel;

using GinRummy.Contracts.DataContracts.PlayerPresence;

namespace GinRummy.Contracts.Services.PlayerPresence
{
    public interface IPlayerPresenceCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnCurrentStatusOfPlayersReported(PlayerPresenceReport report);

        [OperationContract(IsOneWay = true)]
        void OnPlayerPresenceChanged(DataContracts.PlayerPresence.PlayerPresence presence);
    }
}
