using System.Collections.Generic;

namespace GinRummy.Client.Models
{
    /// <summary>
    /// Leaderboard already in the order of CU-19 step 5. The row of the player travels apart
    /// because its place may lie beyond the rows the table holds (CU-19 FA-03).
    /// </summary>
    public sealed class LeaderboardDto
    {
        /// <summary>
        /// Gets or sets the rows from the first place down.
        /// </summary>
        public IList<RankingEntryDto> Entries { get; set; }
        /// <summary>
        /// Gets or sets the row of whoever looks at the leaderboard. When the player is among
        /// the entries it is the same row, not a copy of it.
        /// </summary>
        public RankingEntryDto OwnEntry { get; set; }
    }
}
