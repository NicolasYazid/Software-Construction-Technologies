using System.Collections.Generic;

namespace GinRummy.Client.Models
{
    public sealed class HandResult
    {
        public string WinnerName { get; set; }
        public string KnockerName { get; set; }
        public string DefenderName { get; set; }
        public int KnockerDeadwood { get; set; }
        public int DefenderDeadwood { get; set; }
        public int PointsAwarded { get; set; }
        public IList<Meld> KnockerMelds { get; set; }
        public IList<Meld> DefenderMelds { get; set; }
        public int PlayerScore { get; set; }
        public int OpponentScore { get; set; }
        public string TitleKey { get; set; }
        public string AnnounceKey { get; set; }
        public bool HasScoreBreakdown { get; set; }
        public string BaseName { get; set; }
        public int BaseDeadwood { get; set; }
        public string SubtractedName { get; set; }
        public int SubtractedDeadwood { get; set; }
        public string BonusKey { get; set; }
        public int BonusPoints { get; set; }

        public bool HasSubtraction
        {
            get { return SubtractedName != null; }
        }

        public bool HasBonus
        {
            get { return BonusKey != null; }
        }
    }
}
