using System.Collections.Generic;

namespace GinRummy.Client.Models
{
    // Leaderboard already in the order of CU-19 step 5. The row of the player travels apart
    // because its place may lie beyond the rows the table holds (CU-19 FA-03).
    public sealed class LeaderboardDto
    {
        public IList<RankingEntryDto> Entries { get; set; }
        public RankingEntryDto OwnEntry { get; set; }
    }
}
