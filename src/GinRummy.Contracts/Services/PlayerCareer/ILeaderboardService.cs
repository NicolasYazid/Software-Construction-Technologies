using System.Collections.Generic;
using System.ServiceModel;

using GinRummy.Contracts.DataContracts.PlayerCareer;
using GinRummy.Contracts.Faults.Shared;

namespace GinRummy.Contracts.Services.PlayerCareer
{
    [ServiceContract]
    public interface ILeaderboardService
    {
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        List<PlayerRanking> GetLeaderboard(string accessToken, LeaderboardType leaderboardType);
    }
}
