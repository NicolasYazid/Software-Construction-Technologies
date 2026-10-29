using System.Collections.Generic;

using GinRummy.Domain.Entities;

namespace GinRummy.Application.Ports
{
    /// <summary>
    /// Entry point of the leaderboard process. The network boundary calls it without knowing
    /// which use case answers.
    /// </summary>
    public interface ILeaderboardPort
    {
        /// <summary>
        /// Gets the ordered leaderboard, each position with its place and resolved rank.
        /// </summary>
        /// <returns>The leaderboard positions; empty when there are no players.</returns>
        IReadOnlyList<LeaderboardPosition> GetLeaderboard();
    }
}
