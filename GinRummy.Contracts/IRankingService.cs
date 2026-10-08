using System.Collections.Generic;
using System.ServiceModel;

namespace GinRummy.Contracts
{
    /// <summary>
    /// Network contract that exposes the players' rankings to the client.
    /// </summary>
    [ServiceContract]
    public interface IRankingService
    {
        /// <summary>
        /// Gets the current players' rankings, ordered from highest to lowest score.
        /// </summary>
        /// <returns>The list of ranking rows; empty when there are no players.</returns>
        [OperationContract]
        List<PlayerRankingDto> GetRankings();
    }
}
