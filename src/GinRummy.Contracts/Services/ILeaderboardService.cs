using System.Collections.Generic;
using System.ServiceModel;

using GinRummy.Contracts.DataContracts;

namespace GinRummy.Contracts.Services
{
    /// <summary>
    /// Operations of the leaderboard of the players (CU-19).
    /// </summary>
    [ServiceContract]
    public interface ILeaderboardService
    {
        /// <summary>
        /// Gets the global leaderboard, ordered by the rules of CU-19.
        /// </summary>
        /// <returns>The rows of the leaderboard; empty when there are no players.</returns>
        /// <exception cref="FaultException">Thrown when the server cannot read the leaderboard.</exception>
        [OperationContract]
        List<PlayerRanking> GetLeaderboard();
    }
}
