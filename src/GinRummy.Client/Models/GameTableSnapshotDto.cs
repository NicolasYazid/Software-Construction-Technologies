using System.Collections.Generic;

namespace GinRummy.Client.Models
{
    public sealed class GameTableSnapshotDto
    {
        public string PlayerName { get; set; }
        public string OpponentName { get; set; }
        public int OpponentCardCount { get; set; }
        public int StockCount { get; set; }
        public CardDto DiscardTop { get; set; }
        // The cards keep the order the player arranged them in.
        public IList<CardDto> Hand { get; set; }
        public int Deadwood { get; set; }
        public int PlayerScore { get; set; }
        public int OpponentScore { get; set; }
        public int TargetScore { get; set; }
        public IList<ChatMessageDto> ChatEntries { get; set; }
        public IList<MatchLogEntryDto> MatchLog { get; set; }
    }
}
