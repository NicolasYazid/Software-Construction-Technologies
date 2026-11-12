using System.Collections.Generic;
using System.ServiceModel;

using GinRummy.Contracts.DataContracts.PlayerCareer;

namespace GinRummy.Contracts.Services.PlayerCareer
{
    [ServiceContract]
    public interface ILeaderboardService
    {
        [OperationContract]
        List<PlayerRanking> GetLeaderboard();
    }
}
