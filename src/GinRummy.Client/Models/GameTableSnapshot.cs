using System.Collections.Generic;

namespace GinRummy.Client.Models
{
    public sealed class GameTableSnapshot
    {
        public string PlayerName { get; set; }
        public string OpponentName { get; set; }
        public int OpponentCardCount { get; set; }
        public int StockCount { get; set; }
        public Card DiscardTop { get; set; }
        // The cards keep the order the player arranged them in.
        public IList<Card> Hand { get; set; }
        public int Deadwood { get; set; }
        public int PlayerScore { get; set; }
        public int OpponentScore { get; set; }
        public int TargetScore { get; set; }
        public IList<ChatMessage> ChatEntries { get; set; }
        public IList<MatchLogEntry> MatchLog { get; set; }
    }
}
