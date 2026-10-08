using System.Collections.Generic;

namespace GinRummy.Client.Models
{
    // Close of a hand in which the opponent knocked and the player defended, as the table explains it.
    // It holds who wins the hand, how the points are counted, the groups of both hands and the score of the match afterwards.
    public sealed class HandResultDto
    {
        public string WinnerName { get; set; }
        public string KnockerName { get; set; }
        public int KnockerDeadwood { get; set; }
        public int DefenderDeadwood { get; set; }
        public int PointsAwarded { get; set; }
        public IList<MeldDto> KnockerMelds { get; set; }
        public IList<MeldDto> DefenderMelds { get; set; }
        public int PlayerScore { get; set; }
        public int OpponentScore { get; set; }
    }
}
